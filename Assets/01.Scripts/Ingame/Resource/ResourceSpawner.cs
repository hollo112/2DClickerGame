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

    public Vector2 AreaCenter => _areaCenter;
    public Vector2 AreaSize => _areaSize;
    public IReadOnlyList<GameObject> ActiveResources => _activeResources;

    private void Start()
    {
        UpgradeManager.Instance.OnUpgraded += OnUpgraded;
        for (int i = 0; i < _maxResourceCount; i++) SpawnResource();
    }

    private void OnUpgraded(UpgradeType type, int level) { if (type == UpgradeType.Tool) RemoveOutOfRangeResources(); }

    private void RemoveOutOfRangeResources()
    {
        int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
        int minLevel = Mathf.Max(0, toolLevel - 1);
        int maxLevel = toolLevel + 1;

        for (int i = _activeResources.Count - 1; i >= 0; i--)
        {
            if (_activeResources[i].TryGetComponent(out Resource res))
            {
                if (res.RequiredToolLevel < minLevel || res.RequiredToolLevel > maxLevel)
                {
                    GameObject obj = _activeResources[i];
                    _activeResources.RemoveAt(i);
                    RemoveLevelCount(res.RequiredToolLevel);
                    res.ForceDestroy();
                }
            }
        }
        while (_activeResources.Count < _maxResourceCount) SpawnResource();
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

    private System.Collections.IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnResource();
    }

    private int GetBalancedLevelIndex()
    {
        int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
        int max = _resourcePrefabs.Length - 1;
        int[] levels = { Mathf.Clamp(toolLevel - 1, 0, max), Mathf.Clamp(toolLevel, 0, max), Mathf.Clamp(toolLevel + 1, 0, max) };
        return levels[Random.Range(0, levels.Length)];
    }

    private void AddLevelCount(int l) { if (!_levelCounts.ContainsKey(l)) _levelCounts[l] = 0; _levelCounts[l]++; }
    private void RemoveLevelCount(int l) { if (_levelCounts.ContainsKey(l) && _levelCounts[l] > 0) _levelCounts[l]--; }
}