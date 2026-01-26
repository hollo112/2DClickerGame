using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] _resourcePrefabs;  // 인덱스 = RequiredToolLevel
    [SerializeField] private int _maxResourceCount = 10;
    [SerializeField] private float _minSpacing = 1.5f;
    [SerializeField] private float _respawnDelay = 3f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 _areaCenter;
    [SerializeField] private Vector2 _areaSize = new Vector2(10f, 10f);

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    private List<GameObject> _activeResources = new List<GameObject>();
    private List<Vector2> _occupiedPositions = new List<Vector2>();
    private Dictionary<int, int> _levelCounts = new Dictionary<int, int>();  // 레벨별 개수

    private void Start()
    {
        UpgradeManager.Instance.OnUpgraded += OnUpgraded;
        SpawnInitialResources();
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgraded -= OnUpgraded;
    }

    private void OnUpgraded(UpgradeType type, int level)
    {
        if (type == UpgradeType.Tool)
            RemoveOutOfRangeResources();
    }

    private void RemoveOutOfRangeResources()
    {
        int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
        int maxIndex = _resourcePrefabs.Length - 1;

        int minLevel = Mathf.Clamp(toolLevel - 1, 0, maxIndex);
        int maxLevel = Mathf.Clamp(toolLevel + 1, 0, maxIndex);

        for (int i = _activeResources.Count - 1; i >= 0; i--)
        {
            var obj = _activeResources[i];
            if (obj != null && obj.TryGetComponent(out Resource res))
            {
                if (res.RequiredToolLevel < minLevel || res.RequiredToolLevel > maxLevel)
                {
                    _occupiedPositions.RemoveAt(i);
                    _activeResources.RemoveAt(i);
                    RemoveLevelCount(res.RequiredToolLevel);
                    Destroy(obj);
                    StartCoroutine(RespawnAfterDelay());
                }
            }
        }
    }

    private void SpawnInitialResources()
    {
        for (int i = 0; i < _maxResourceCount; i++)
        {
            SpawnResource();
        }
    }

    public void SpawnResource()
    {
        if (_activeResources.Count >= _maxResourceCount) return;

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null)
        {
            Debug.LogWarning("[ResourceSpawner] 유효한 스폰 위치를 찾지 못했습니다.");
            return;
        }

        int prefabIndex = GetBalancedLevelIndex();
        GameObject prefab = _resourcePrefabs[prefabIndex];
        GameObject resource = Instantiate(prefab, spawnPos.Value, Quaternion.identity, transform);

        // 좌우 방향 랜덤
        float direction = Random.value > 0.5f ? 1f : -1f;
        Vector3 scale = resource.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        resource.transform.localScale = scale;

        // Feedback에 원본 scale 설정
        if (resource.TryGetComponent(out ScaleTweeningFeedback feedback))
        {
            feedback.SetOriginalScale(scale);
        }

        _activeResources.Add(resource);
        _occupiedPositions.Add(spawnPos.Value);

        // 레벨 카운트 증가
        AddLevelCount(prefabIndex);

        if (resource.TryGetComponent(out Resource res))
        {
            res.Initialize(this, prefabIndex);
        }
    }

    private int GetBalancedLevelIndex()
    {
        int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
        int maxIndex = _resourcePrefabs.Length - 1;

        // 3개 레벨: toolLevel-1, toolLevel, toolLevel+1
        int[] targetLevels = new int[3];
        targetLevels[0] = Mathf.Clamp(toolLevel - 1, 0, maxIndex);
        targetLevels[1] = Mathf.Clamp(toolLevel, 0, maxIndex);
        targetLevels[2] = Mathf.Clamp(toolLevel + 1, 0, maxIndex);

        // 각 레벨의 목표 개수 (전체의 1/3)
        int targetCountPerLevel = Mathf.CeilToInt(_maxResourceCount / 3f);

        // 부족한 레벨 찾기
        List<int> underfilledLevels = new List<int>();
        foreach (int level in targetLevels)
        {
            int currentCount = GetLevelCount(level);
            if (currentCount < targetCountPerLevel)
            {
                underfilledLevels.Add(level);
            }
        }

        // 부족한 레벨 중 랜덤 선택, 없으면 전체 중 랜덤
        if (underfilledLevels.Count > 0)
        {
            return underfilledLevels[Random.Range(0, underfilledLevels.Count)];
        }

        return targetLevels[Random.Range(0, targetLevels.Length)];
    }

    private int GetLevelCount(int level)
    {
        return _levelCounts.TryGetValue(level, out int count) ? count : 0;
    }

    private void AddLevelCount(int level)
    {
        if (!_levelCounts.ContainsKey(level))
            _levelCounts[level] = 0;
        _levelCounts[level]++;
    }

    private void RemoveLevelCount(int level)
    {
        if (_levelCounts.ContainsKey(level) && _levelCounts[level] > 0)
            _levelCounts[level]--;
    }

    private Vector2? FindValidSpawnPosition(int maxAttempts = 30)
    {
        Vector2 min = _areaCenter - _areaSize / 2f;
        Vector2 max = _areaCenter + _areaSize / 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );

            if (IsValidPosition(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private bool IsValidPosition(Vector2 position)
    {
        foreach (Vector2 occupied in _occupiedPositions)
        {
            if (Vector2.Distance(position, occupied) < _minSpacing)
            {
                return false;
            }
        }
        return true;
    }

    // 자원이 파괴될 때 호출
    public void OnResourceDestroyed(GameObject resource, Vector2 position, int level)
    {
        _activeResources.Remove(resource);
        _occupiedPositions.Remove(position);
        RemoveLevelCount(level);

        // 일정 시간 후 리스폰
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

        // 스폰 영역 표시
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(_areaCenter, _areaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_areaCenter, _areaSize);
    }
}