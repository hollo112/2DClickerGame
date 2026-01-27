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
        int maxIdx = _resourcePrefabs.Length - 1;

        // 소환 가능한 후보군 설정: [레벨-1, 레벨, 레벨+1]
        int[] candidates = {
            Mathf.Clamp(toolLevel - 1, 0, maxIdx),
            Mathf.Clamp(toolLevel, 0, maxIdx),
            Mathf.Clamp(toolLevel + 1, 0, maxIdx)
        };

        // 중복 제거된 후보 레벨들 (도구 레벨이 0일 경우 0, 1만 남음)
        var distinctLevels = candidates.Distinct().ToList();

        // 1. 각 레벨이 목표치(총량의 1/3)에 도달했는지 확인
        // 목표치보다 현재 개수가 적은 레벨들만 따로 선별
        int targetCountPerLevel = _maxResourceCount / distinctLevels.Count;

        List<int> underPopulatedLevels = new List<int>();

        foreach (int level in distinctLevels)
        {
            _levelCounts.TryGetValue(level, out int currentCount);
            if (currentCount < targetCountPerLevel)
            {
                underPopulatedLevels.Add(level);
            }
        }

        // 2. 만약 목표치보다 적은 레벨이 있다면 그 중에서 랜덤 선택 (부족한 곳 채우기)
        if (underPopulatedLevels.Count > 0)
        {
            return underPopulatedLevels[Random.Range(0, underPopulatedLevels.Count)];
        }

        // 3. 모든 레벨이 균등하게 찼다면 전체 후보 중 완전 랜덤 선택
        return candidates[Random.Range(0, candidates.Length)];
    }

    private void AddLevelCount(int l) { if (!_levelCounts.ContainsKey(l)) _levelCounts[l] = 0; _levelCounts[l]++; }
    private void RemoveLevelCount(int l) { if (_levelCounts.ContainsKey(l) && _levelCounts[l] > 0) _levelCounts[l]--; }
}