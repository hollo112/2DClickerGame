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

    public event Action OnMonsterChanged;

    private const int MonstersRequiredForMerge = 3;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region Spawn

    public bool CanSpawn()
    {
        if (_data == null) return false;
        if (!CurrencyManager.Instance.CanAfford(_data.SpawnCost)) return false;
        if (GetTierCount(0) >= _data.MaxMonstersPerTier) return false;

        return true;
    }

    public bool TrySpawnMonster()
    {
        if (!ValidateSpawnConditions()) return false;

        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null)
        {
            Debug.LogWarning("[MonsterManager] 유효한 스폰 위치를 찾지 못했습니다.");
            return false;
        }

        CurrencyManager.Instance.SpendMoney(_data.SpawnCost);
        return CreateMonster(0, spawnPos.Value);
    }

    private bool ValidateSpawnConditions()
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

        if (GetTierCount(0) >= _data.MaxMonstersPerTier)
        {
            Debug.Log("[MonsterManager] 1단계 몬스터가 최대입니다.");
            return false;
        }

        return true;
    }

    #endregion

    #region Merge

    public bool CanMerge()
    {
        if (_data == null) return false;
        if (!CurrencyManager.Instance.CanAfford(_data.MergeCost)) return false;

        return FindMergeableTier() >= 0;
    }

    public bool TryMerge()
    {
        if (!CurrencyManager.Instance.CanAfford(_data.MergeCost))
        {
            Debug.Log("[MonsterManager] 머지 비용이 부족합니다.");
            return false;
        }

        int targetTier = FindMergeableTier();
        if (targetTier < 0)
        {
            Debug.Log("[MonsterManager] 머지할 수 있는 몬스터가 없습니다.");
            return false;
        }

        var monstersToMerge = GetMonstersOfTier(targetTier, MonstersRequiredForMerge);
        if (monstersToMerge.Count < MonstersRequiredForMerge)
            return false;

        CurrencyManager.Instance.SpendMoney(_data.MergeCost);

        Vector2 mergePosition = CalculateCenterPosition(monstersToMerge);
        RemoveAndDestroyMonsters(monstersToMerge);

        int newTier = targetTier + 1;
        bool success = CreateMonster(newTier, mergePosition);

        if (success)
            Debug.Log($"[MonsterManager] 머지 완료! {targetTier + 1}단계 → {newTier + 1}단계");

        return success;
    }

    private int FindMergeableTier()
    {
        for (int tier = 0; tier < _data.MaxTier; tier++)
        {
            if (GetTierCount(tier) < MonstersRequiredForMerge)
                continue;

            int newTier = tier + 1;
            if (GetTierCount(newTier) >= _data.MaxMonstersPerTier)
                continue;

            return tier;
        }
        return -1;
    }

    private List<Monster> GetMonstersOfTier(int tier, int count)
    {
        return _monsters
            .Where(m => m != null && m.Tier == tier)
            .Take(count)
            .ToList();
    }

    private Vector2 CalculateCenterPosition(List<Monster> monsters)
    {
        Vector2 center = Vector2.zero;
        foreach (var m in monsters)
            center += m.Position;
        return center / monsters.Count;
    }

    private void RemoveAndDestroyMonsters(List<Monster> monsters)
    {
        foreach (var m in monsters)
        {
            RemoveMonster(m);
            m.OnMerged();
        }
    }

    #endregion

    #region Monster Creation

    private bool CreateMonster(int tier, Vector2 position)
    {
        GameObject monsterObj = Instantiate(_data.MonsterPrefab, position, Quaternion.identity, transform);

        if (monsterObj.TryGetComponent(out Monster monster))
        {
            monster.Initialize(this, tier, _data.Tiers[tier]);
            RegisterMonster(monster, position);
            OnMonsterChanged?.Invoke();
            return true;
        }

        Destroy(monsterObj);
        return false;
    }

    #endregion

    #region Monster Registry

    private void RegisterMonster(Monster monster, Vector2 position)
    {
        _monsters.Add(monster);
        _occupiedPositions.Add(position);
    }

    public void RemoveMonster(Monster monster)
    {
        int index = _monsters.IndexOf(monster);
        if (index < 0) return;

        _monsters.RemoveAt(index);
        if (index < _occupiedPositions.Count)
            _occupiedPositions.RemoveAt(index);
    }

    public void UpdateMonsterPosition(Monster monster, Vector2 newPosition)
    {
        int index = _monsters.IndexOf(monster);
        if (index >= 0 && index < _occupiedPositions.Count)
            _occupiedPositions[index] = newPosition;
    }

    private int GetTierCount(int tier)
    {
        CleanupNullMonsters();
        return _monsters.Count(m => m != null && m.Tier == tier);
    }

    private void CleanupNullMonsters()
    {
        for (int i = _monsters.Count - 1; i >= 0; i--)
        {
            if (_monsters[i] == null)
            {
                _monsters.RemoveAt(i);
                if (i < _occupiedPositions.Count)
                    _occupiedPositions.RemoveAt(i);
            }
        }
    }

    #endregion

    #region Position Validation

    private Vector2? FindValidSpawnPosition(int maxAttempts = 30)
    {
        if (_resourceSpawner == null) return null;

        Vector2 min = _resourceSpawner.AreaCenter - _resourceSpawner.AreaSize / 2f;
        Vector2 max = _resourceSpawner.AreaCenter + _resourceSpawner.AreaSize / 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );

            if (IsValidPosition(candidate))
                return candidate;
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
                return false;
        }

        // Resource와의 거리 검사
        if (_resourceSpawner != null)
        {
            foreach (Vector2 resourcePos in _resourceSpawner.OccupiedPositions)
            {
                if (Vector2.Distance(position, resourcePos) < spacing)
                    return false;
            }
        }

        return true;
    }

    #endregion

    #region Target Finding

    public Resource FindRandomResource(int maxResourceLevel)
    {
        var validResources = GetValidResources(maxResourceLevel);
        if (validResources.Count == 0) return null;

        var untargetedResources = FilterUntargetedResources(validResources);
        var pool = untargetedResources.Count > 0 ? untargetedResources : validResources;

        return pool[Random.Range(0, pool.Count)];
    }

    private List<Resource> GetValidResources(int maxLevel)
    {
        return FindObjectsOfType<Resource>()
            .Where(r => r.RequiredToolLevel <= maxLevel)
            .ToList();
    }

    private List<Resource> FilterUntargetedResources(List<Resource> resources)
    {
        var targetedSet = GetTargetedResources();
        return resources.Where(r => !targetedSet.Contains(r)).ToList();
    }

    private HashSet<Resource> GetTargetedResources()
    {
        var targeted = new HashSet<Resource>();
        foreach (var monster in _monsters)
        {
            if (monster != null && monster.TargetResource != null)
                targeted.Add(monster.TargetResource);
        }
        return targeted;
    }

    #endregion
}
