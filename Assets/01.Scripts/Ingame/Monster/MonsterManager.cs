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
    public MonsterData Data => _data;
    public IReadOnlyList<Monster> Monsters => _monsters;

    public event Action OnMonsterChanged;
    private const int MonstersRequiredForMerge = 3;

    private int _currentToolLevel;
    public int CurrentToolLevel => _currentToolLevel;

    private IMonsterRepository _repository;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _repository = new LocalMonsterRepository();
    }

    private void Start()
    {
        _currentToolLevel = UpgradeManager.Instance?.Get(EUpgradeType.ToolLevel)?.Level ?? 0;
        UpgradeManager.OnDataChanged += OnUpgradeChanged;

        Load();
    }

    private void OnDestroy()
    {
        UpgradeManager.OnDataChanged -= OnUpgradeChanged;
    }

    private void OnUpgradeChanged()
    {
        int newLevel = UpgradeManager.Instance?.Get(EUpgradeType.ToolLevel)?.Level ?? 0;
        if (_currentToolLevel == newLevel) return;
        _currentToolLevel = newLevel;
    }

    #region Save & Load
    private void Save()
    {
        if (_data == null) return;

        var saveData = new MonsterSaveData
        {
            TierCounts = new int[_data.Tiers.Length]
        };
        for (int i = 0; i < _data.Tiers.Length; i++)
        {
            saveData.TierCounts[i] = GetTierCount(i);
        }
        _repository.Save(saveData);
    }

    private void Load()
    {
        var saveData = _repository.Load();
        if (saveData.TierCounts == null || saveData.TierCounts.Length == 0) return;

        int tierCount = Mathf.Min(saveData.TierCounts.Length, _data.Tiers.Length);
        for (int tier = 0; tier < tierCount; tier++)
        {
            for (int i = 0; i < saveData.TierCounts[tier]; i++)
            {
                Vector2? pos = FindValidSpawnPosition();
                if (pos != null)
                {
                    CreateMonster(tier, pos.Value);
                }
            }
        }
    }
    #endregion

    #region Spawn & Merge Logic
    public bool CanSpawn() => _data != null && CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, _data.SpawnCost) && GetTierCount(0) < _data.MaxMonstersPerTier;

    public bool TrySpawnMonster()
    {
        if (!CanSpawn()) return false;
        Vector2? spawnPos = FindValidSpawnPosition();
        if (spawnPos == null) return false;

        CurrencyManager.Instance.Spend(ECurrencyType.Gold, _data.SpawnCost);
        bool success = CreateMonster(0, spawnPos.Value);
        if (success) Save();
        return success;
    }

    public bool CanMerge()
    {
        if (_data == null) return false;
        int tier = FindMergeableTier();
        if (tier < 0) return false;
        return CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, GetMergeCost(tier));
    }

    public double GetMergeCost(int tier)
    {
        if (_data.MergeCosts == null || tier < 0 || tier >= _data.MergeCosts.Length)
            return double.MaxValue;
        return _data.MergeCosts[tier];
    }

    public int GetMergeableTier() => FindMergeableTier();

    public bool TryMerge()
    {
        int targetTier = FindMergeableTier();
        if (targetTier < 0) return false;

        double cost = GetMergeCost(targetTier);
        if (!CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, cost)) return false;

        var targets = _monsters.Where(m => m != null && m.Tier == targetTier).Take(MonstersRequiredForMerge).ToList();
        if (targets.Count < MonstersRequiredForMerge) return false;

        CurrencyManager.Instance.Spend(ECurrencyType.Gold, cost);
        Vector2 mergePos = Vector2.zero;
        foreach (var m in targets) mergePos += (Vector2)m.transform.position;
        mergePos /= targets.Count;

        foreach (var m in targets) { _monsters.Remove(m); m.OnMerged(); }

        bool success = CreateMonster(targetTier + 1, mergePos);
        if (success)
        {
            Save();
            OnMonsterChanged?.Invoke();
        }
        return success;
    }

    private int FindMergeableTier()
    {
        for (int i = 0; i < _data.MaxTier; i++)
        {
            if (GetTierCount(i) >= MonstersRequiredForMerge)
            {
                if (i + 1 < _data.Tiers.Length && GetTierCount(i + 1) < _data.MaxMonstersPerTier) return i;
            }
        }
        return -1;
    }
    #endregion

    #region Monster Management
    private bool CreateMonster(int tier, Vector2 position)
    {
        GameObject obj = Instantiate(_data.MonsterPrefab, position, Quaternion.identity, transform);
        if (obj.TryGetComponent(out Monster monster))
        {
            // 1. 초기화 전 리스트 등록 (가장 먼저!)
            if (!_monsters.Contains(monster))
            {
                _monsters.Add(monster);
            }

            // 2. 초기화 진행
            monster.Initialize(this, tier, _data.Tiers[tier]);

            OnMonsterChanged?.Invoke(); // UI 갱신 알림
            return true;
        }
        Destroy(obj);
        return false;
    }

    public void RemoveMonster(Monster monster)
    {
        if (_monsters.Remove(monster))
        {
            Save();
            OnMonsterChanged?.Invoke();
        }
    }

    public int GetTierCount(int tier)
    {
        _monsters.RemoveAll(m => m == null);
        return _monsters.Count(m => m.Tier == tier);
    }
    #endregion

    #region Position Validation
    private Vector2? FindValidSpawnPosition()
    {
        Vector2 min = _resourceSpawner.AreaCenter - _resourceSpawner.AreaSize / 2f;
        Vector2 max = _resourceSpawner.AreaCenter + _resourceSpawner.AreaSize / 2f;

        for (int i = 0; i < 50; i++)
        {
            Vector2 candidate = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            if (IsValidPosition(candidate)) return candidate;
        }
        return null;
    }

    private bool IsValidPosition(Vector2 pos)
    {
        float spacing = _data.MinSpacing;
        // 실시간 몬스터 위치 체크
        if (_monsters.Any(m => m != null && Vector2.Distance(pos, m.transform.position) < spacing)) return false;
        // 실시간 자원 위치 체크
        if (_resourceSpawner.ActiveResources.Any(r => r != null && Vector2.Distance(pos, r.transform.position) < spacing)) return false;
        return true;
    }
    #endregion

    public Resource FindRandomResource(int maxLevel)
    {
        var resources = FindObjectsOfType<Resource>().Where(r => r.RequiredToolLevel <= maxLevel).ToList();
        if (resources.Count == 0) return null;
        var targeted = new HashSet<Resource>(_monsters.Where(m => m != null && m.TargetResource != null).Select(m => m.TargetResource));
        var untargeted = resources.Where(r => !targeted.Contains(r)).ToList();
        return (untargeted.Count > 0 ? untargeted : resources)[Random.Range(0, (untargeted.Count > 0 ? untargeted : resources).Count)];
    }
}
