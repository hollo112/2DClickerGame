using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] _resourcePrefabs;
    [SerializeField] private int _maxResourceCount = 15;
    [SerializeField] private float _minSpacing = 1.5f;
    [SerializeField] private float _respawnDelay = 3f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 _areaCenter;
    [SerializeField] private Vector2 _areaSize = new Vector2(10f, 10f);

    private List<GameObject> _activeResources = new List<GameObject>();
    private Dictionary<int, int> _levelCounts = new Dictionary<int, int>();
    private int _currentToolLevel;

    public Vector2 AreaCenter => _areaCenter;
    public Vector2 AreaSize => _areaSize;
    public IReadOnlyList<GameObject> ActiveResources => _activeResources;

    private void Start()
    {
        for (int i = 0; i < _maxResourceCount; i++) SpawnResource();
    }

    public void SetToolLevel(int level)
    {
        if (_currentToolLevel == level) return;
        _currentToolLevel = level;
        RespawnAll();
    }

    private void RespawnAll()
    {
        StopAllCoroutines();

        for (int i = _activeResources.Count - 1; i >= 0; i--)
        {
            var resObj = _activeResources[i];
            if (resObj != null && resObj.TryGetComponent(out Resource res))
                res.ForceDestroy();
        }
        _activeResources.Clear();
        _levelCounts.Clear();

        for (int i = 0; i < _maxResourceCount; i++)
            SpawnResource();
    }

    public void SpawnResource()
    {
        if (_activeResources.Count >= _maxResourceCount) return;
        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null) return;

        int index = GetBalancedLevelIndex();
        GameObject resObj = Instantiate(_resourcePrefabs[index], spawnPos.Value, Quaternion.identity, transform);

        _activeResources.Add(resObj);
        AddLevelCount(index);
        if (resObj.TryGetComponent(out Resource res)) res.Initialize(this, index);
    }

    private Vector2? FindValidSpawnPosition()
    {
        Vector2 min = _areaCenter - _areaSize / 2f;
        Vector2 max = _areaCenter + _areaSize / 2f;
        for (int i = 0; i < 30; i++)
        {
            Vector2 candidate = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (IsValidPosition(candidate)) return candidate;
        }
        return null;
    }

    private bool IsValidPosition(Vector2 pos)
    {
        if (_activeResources.Any(r => r != null && Vector2.Distance(pos, r.transform.position) < _minSpacing)) return false;
        if (MonsterManager.Instance != null && MonsterManager.Instance.Monsters.Any(m => m != null && Vector2.Distance(pos, m.transform.position) < _minSpacing)) return false;
        return true;
    }

    public void OnResourceDestroyed(GameObject resource, Vector2 position, int level)
    {
        _activeResources.Remove(resource);
        RemoveLevelCount(level);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnResource();
    }

    private int GetBalancedLevelIndex()
    {
        int maxIdx = _resourcePrefabs.Length - 1;
        int min = Mathf.Max(0, _currentToolLevel - 1);
        int max = Mathf.Min(maxIdx, _currentToolLevel + 1);

        List<int> validLevels = new List<int>();
        for (int i = min; i <= max; i++)
            validLevels.Add(i);

        int targetPerLevel = _maxResourceCount / validLevels.Count;

        List<int> underPopulated = new List<int>();
        foreach (int level in validLevels)
        {
            _levelCounts.TryGetValue(level, out int count);
            if (count < targetPerLevel)
                underPopulated.Add(level);
        }

        if (underPopulated.Count > 0)
            return underPopulated[Random.Range(0, underPopulated.Count)];

        return validLevels[Random.Range(0, validLevels.Count)];
    }

    private void AddLevelCount(int l) { if (!_levelCounts.ContainsKey(l)) _levelCounts[l] = 0; _levelCounts[l]++; }
    private void RemoveLevelCount(int l) { if (_levelCounts.ContainsKey(l) && _levelCounts[l] > 0) _levelCounts[l]--; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(_areaCenter, _areaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_areaCenter, _areaSize);
    }
}
