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

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    private List<GameObject> _activeResources = new List<GameObject>();
    private Dictionary<int, int> _levelCounts = new Dictionary<int, int>();

    public Vector2 AreaCenter => _areaCenter;
    public Vector2 AreaSize => _areaSize;
    
    public IReadOnlyList<GameObject> ActiveResources => _activeResources;

    private void Start()
    {
        UpgradeManager.Instance.OnUpgraded += OnUpgraded;
        SpawnInitialResources();
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.OnUpgraded -= OnUpgraded;
    }

    private void OnUpgraded(UpgradeType type, int level)
    {
        if (type == UpgradeType.Tool) RemoveOutOfRangeResources();
    }

    private void RemoveOutOfRangeResources()
    {
        int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
        int maxIndex = _resourcePrefabs.Length - 1;
        int minLevel = Mathf.Clamp(toolLevel - 1, 0, maxIndex);
        int maxLevel = Mathf.Clamp(toolLevel + 1, 0, maxIndex);

        int removeCount = 0;
        for (int i = _activeResources.Count - 1; i >= 0; i--)
        {
            var obj = _activeResources[i];
            if (obj != null && obj.TryGetComponent(out Resource res))
            {
                if (res.RequiredToolLevel < minLevel || res.RequiredToolLevel > maxLevel)
                {
                    _activeResources.RemoveAt(i);
                    RemoveLevelCount(res.RequiredToolLevel);
                    res.ForceDestroy();
                    removeCount++;
                }
            }
        }
        for (int i = 0; i < removeCount; i++) SpawnResource();
    }

    private void SpawnInitialResources() { for (int i = 0; i < _maxResourceCount; i++) SpawnResource(); }

    public void SpawnResource()
    {
        if (_activeResources.Count >= _maxResourceCount) return;

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null) return;

        int prefabIndex = GetBalancedLevelIndex();
        GameObject prefab = _resourcePrefabs[prefabIndex];
        GameObject resource = Instantiate(prefab, spawnPos.Value, Quaternion.identity, transform);

        float direction = Random.value > 0.5f ? 1f : -1f;
        Vector3 scale = resource.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        resource.transform.localScale = scale;

        if (resource.TryGetComponent(out ScaleTweeningFeedback feedback)) feedback.SetOriginalScale(scale);

        _activeResources.Add(resource);
        AddLevelCount(prefabIndex);

        if (resource.TryGetComponent(out Resource res)) res.Initialize(this, prefabIndex);
    }

    private int GetBalancedLevelIndex()
    {
        int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
        int maxIndex = _resourcePrefabs.Length - 1;
        int[] targetLevels = {
            Mathf.Clamp(toolLevel - 1, 0, maxIndex),
            Mathf.Clamp(toolLevel, 0, maxIndex),
            Mathf.Clamp(toolLevel + 1, 0, maxIndex)
        };

        int targetCountPerLevel = Mathf.CeilToInt(_maxResourceCount / 3f);
        var underfilled = targetLevels.Where(lvl => GetLevelCount(lvl) < targetCountPerLevel).ToList();

        return underfilled.Count > 0 ? underfilled[Random.Range(0, underfilled.Count)] : targetLevels[Random.Range(0, targetLevels.Length)];
    }

    private int GetLevelCount(int level) => _levelCounts.TryGetValue(level, out int count) ? count : 0;
    private void AddLevelCount(int level) { if (!_levelCounts.ContainsKey(level)) _levelCounts[level] = 0; _levelCounts[level]++; }
    private void RemoveLevelCount(int level) { if (_levelCounts.ContainsKey(level) && _levelCounts[level] > 0) _levelCounts[level]--; }

    private Vector2? FindValidSpawnPosition(int maxAttempts = 30)
    {
        Vector2 min = _areaCenter - _areaSize / 2f;
        Vector2 max = _areaCenter + _areaSize / 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (IsValidPosition(candidate)) return candidate;
        }
        return null;
    }

    private bool IsValidPosition(Vector2 position)
    {
        // 1. 자원끼리 거리 체크
        foreach (var obj in _activeResources)
        {
            if (obj != null && Vector2.Distance(position, obj.transform.position) < _minSpacing) return false;
        }

        // 2. 몬스터 매니저의 실시간 리스트를 통한 거리 체크
        if (MonsterManager.Instance != null)
        {
            foreach (var monster in MonsterManager.Instance.Monsters)
            {
                if (monster != null && Vector2.Distance(position, monster.transform.position) < _minSpacing) return false;
            }
        }
        return true;
    }

    public void OnResourceDestroyed(GameObject resource, Vector2 position, int level)
    {
        _activeResources.Remove(resource);
        RemoveLevelCount(level);
        StartCoroutine(RespawnAfterDelay());
    }

    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnResource();
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(_areaCenter, _areaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_areaCenter, _areaSize);
    }
}