using System;
using UnityEngine;

public class MonsterOutgameManager : MonoBehaviour
{
    public static MonsterOutgameManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [SerializeField] private MonsterData _data;

    private MonsterSpec _spec;
    private MonsterCollection _collection;
    private IMonsterRepository _repository;

    private const int MonstersRequiredForMerge = 3;

    public MonsterSpec Spec => _spec;

    /// <summary>
    /// 스폰 비용 (유효성 보장됨)
    /// </summary>
    public double SpawnCost => _spec?.SpawnCost ?? double.MaxValue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // MonsterSpec 생성 (유효성 검사 포함)
        _spec = new MonsterSpec(_data);

        _repository = new FirebaseMonsterRepository(AccountManager.Instance.Email);
        _collection = new MonsterCollection(_spec.TierCount, _spec.MaxMonstersPerTier, MonstersRequiredForMerge);
        Load();
    }

    /// <summary>
    /// 티어별 머지 비용을 반환합니다 (유효성 보장됨)
    /// </summary>
    public double GetMergeCost(int tier)
    {
        return _spec.GetMergeCost(tier);
    }

    /// <summary>
    /// 머지 가능한 티어를 찾습니다
    /// </summary>
    public int GetMergeableTier() => _collection.FindMergeableTier();

    public int GetTierCount(int tier) => _collection.GetTierCount(tier);

    public bool CanSpawn()
    {
        return _spec.CanSpawn()
            && _collection.CanAddMonster(0)
            && CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, _spec.SpawnCost);
    }

    public bool TrySpawn()
    {
        if (!CanSpawn()) return false;

        CurrencyManager.Instance.Spend(ECurrencyType.Gold, _spec.SpawnCost);
        _collection.AddMonster(0);
        Save();
        OnDataChanged?.Invoke();
        return true;
    }

    public bool CanMerge()
    {
        if (!_spec.CanSpawn()) return false;

        int tier = _collection.FindMergeableTier();
        if (tier < 0) return false;

        return _spec.CanMerge(tier)
            && CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, _spec.GetMergeCost(tier));
    }

    public bool TryMerge(out int sourceTier)
    {
        sourceTier = _collection.FindMergeableTier();
        if (sourceTier < 0) return false;

        if (!_spec.CanMerge(sourceTier)) return false;

        double cost = _spec.GetMergeCost(sourceTier);
        if (!CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, cost)) return false;

        CurrencyManager.Instance.Spend(ECurrencyType.Gold, cost);

        for (int i = 0; i < MonstersRequiredForMerge; i++)
            _collection.RemoveMonster(sourceTier);
        _collection.AddMonster(sourceTier + 1);

        Save();
        OnDataChanged?.Invoke();
        return true;
    }

    private void Save()
    {
        var saveData = new MonsterSaveData
        {
            TierCounts = _collection.GetAllTierCounts()
        };
        _repository.Save(saveData).Forget();
    }

    private async void Load()
    {
        var saveData = await _repository.Load();
        _collection.SetTierCounts(saveData.TierCounts);
        OnDataChanged?.Invoke();
    }
}
