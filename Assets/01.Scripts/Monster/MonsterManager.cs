using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [SerializeField] private MonsterData _data;
    [SerializeField] private ResourceSpawner _resourceSpawner;

    private List<Monster> _monsters = new List<Monster>();
    private List<Vector2> _occupiedPositions = new List<Vector2>();

    public MonsterData Data => _data;
    public IReadOnlyList<Monster> Monsters => _monsters;
    public IReadOnlyList<Vector2> OccupiedPositions => _occupiedPositions;

    // 몬스터 상태 변경 이벤트 (UI 갱신용)
    public event Action OnMonsterChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool TrySpawnMonster()
    {
        if (_data == null || _data.MonsterPrefab == null)
        {
            Debug.LogWarning("[MonsterManager] MonsterData 또는 프리팹이 설정되지 않았습니다.");
            return false;
        }

        if (!CurrencyManager.Instance.CanAfford(_data.SpawnCost))
        {
            Debug.Log("[MonsterManager] 소환 비용이 부족합니다.");
            return false;
        }

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null)
        {
            Debug.LogWarning("[MonsterManager] 유효한 스폰 위치를 찾지 못했습니다.");
            return false;
        }

        CurrencyManager.Instance.SpendMoney(_data.SpawnCost);

        GameObject monsterObj = Instantiate(_data.MonsterPrefab, spawnPos.Value, Quaternion.identity, transform);

        if (monsterObj.TryGetComponent(out Monster monster))
        {
            monster.Initialize(this, 0, _data.Tiers[0]);  // 1단계 (tier 0)
            _monsters.Add(monster);
            _occupiedPositions.Add(spawnPos.Value);
            Debug.Log($"[MonsterManager] 1단계 몬스터 소환! 위치: {spawnPos.Value}");
            OnMonsterChanged?.Invoke();
            return true;
        }

        Destroy(monsterObj);
        return false;
    }

    public bool CanMerge()
    {
        // 머지 가능한 티어 찾기 (5단계 미만인 몬스터 중 3마리 이상 있는 것)
        var tierCounts = GetTierCounts();

        foreach (var pair in tierCounts)
        {
            // 최대 티어(4, 0-indexed)가 아니고 3마리 이상인 경우
            if (pair.Key < _data.MaxTier && pair.Value >= 3)
            {
                return CurrencyManager.Instance.CanAfford(_data.MergeCost);
            }
        }

        return false;
    }

    public bool TryMerge()
    {
        if (!CurrencyManager.Instance.CanAfford(_data.MergeCost))
        {
            Debug.Log("[MonsterManager] 머지 비용이 부족합니다.");
            return false;
        }

        // 가장 낮은 티어 중 3마리 이상 있는 티어 찾기
        var tierCounts = GetTierCounts();
        int targetTier = -1;

        for (int tier = 0; tier <= _data.MaxTier; tier++)
        {
            if (tierCounts.TryGetValue(tier, out int count) && count >= 3 && tier < _data.MaxTier)
            {
                targetTier = tier;
                break;
            }
        }

        if (targetTier < 0)
        {
            Debug.Log("[MonsterManager] 머지할 수 있는 몬스터가 없습니다.");
            return false;
        }

        // 해당 티어 몬스터 3마리 선택
        var monstersToMerge = _monsters
            .Where(m => m.Tier == targetTier)
            .Take(3)
            .ToList();

        if (monstersToMerge.Count < 3)
        {
            return false;
        }

        CurrencyManager.Instance.SpendMoney(_data.MergeCost);

        // 머지 위치 계산 (3마리의 중심)
        Vector2 mergePosition = Vector2.zero;
        foreach (var m in monstersToMerge)
        {
            mergePosition += m.Position;
        }
        mergePosition /= 3f;

        // 3마리 제거
        foreach (var m in monstersToMerge)
        {
            RemoveMonster(m);
            m.OnMerged();
        }

        // 새로운 상위 티어 몬스터 생성
        int newTier = targetTier + 1;
        GameObject monsterObj = Instantiate(_data.MonsterPrefab, mergePosition, Quaternion.identity, transform);

        if (monsterObj.TryGetComponent(out Monster monster))
        {
            monster.Initialize(this, newTier, _data.Tiers[newTier]);
            _monsters.Add(monster);
            _occupiedPositions.Add(mergePosition);
            Debug.Log($"[MonsterManager] 머지 완료! {targetTier + 1}단계 → {newTier + 1}단계");
            OnMonsterChanged?.Invoke();
            return true;
        }

        Destroy(monsterObj);
        return false;
    }

    private Dictionary<int, int> GetTierCounts()
    {
        var counts = new Dictionary<int, int>();
        foreach (var monster in _monsters)
        {
            if (!counts.ContainsKey(monster.Tier))
                counts[monster.Tier] = 0;
            counts[monster.Tier]++;
        }
        return counts;
    }

    private Vector2? FindValidSpawnPosition(int maxAttempts = 30)
    {
        if (_resourceSpawner == null) return null;

        Vector2 areaCenter = _resourceSpawner.AreaCenter;
        Vector2 areaSize = _resourceSpawner.AreaSize;

        Vector2 min = areaCenter - areaSize / 2f;
        Vector2 max = areaCenter + areaSize / 2f;

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
        float spacing = _data.MinSpacing;

        // 다른 Monster와의 거리 검사
        foreach (Vector2 occupied in _occupiedPositions)
        {
            if (Vector2.Distance(position, occupied) < spacing)
            {
                return false;
            }
        }

        // Resource와의 거리 검사
        if (_resourceSpawner != null)
        {
            foreach (Vector2 resourcePos in _resourceSpawner.OccupiedPositions)
            {
                if (Vector2.Distance(position, resourcePos) < spacing)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void UpdateMonsterPosition(Monster monster, Vector2 newPosition)
    {
        int index = _monsters.IndexOf(monster);
        if (index >= 0 && index < _occupiedPositions.Count)
        {
            _occupiedPositions[index] = newPosition;
        }
    }

    public void RemoveMonster(Monster monster)
    {
        int index = _monsters.IndexOf(monster);
        if (index >= 0)
        {
            _monsters.RemoveAt(index);
            if (index < _occupiedPositions.Count)
            {
                _occupiedPositions.RemoveAt(index);
            }
        }
    }

    public Resource FindClosestResource(Vector2 position, int targetResourceLevel)
    {
        Resource closest = null;
        float closestDistance = float.MaxValue;

        var resources = FindObjectsOfType<Resource>();
        foreach (var resource in resources)
        {
            // 정확히 해당 레벨의 Resource만 타겟으로
            if (resource.RequiredToolLevel != targetResourceLevel)
                continue;

            float distance = Vector2.Distance(position, resource.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = resource;
            }
        }

        return closest;
    }
}
