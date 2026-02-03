using System;
using UnityEngine;

public class MonsterOutgameManager : MonoBehaviour
{
    public static MonsterOutgameManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [SerializeField] private MonsterData _data;

    private MonsterCollection _collection;
    private IMonsterRepository _repository;

    private const int MonstersRequiredForMerge = 3;

    public MonsterData Data => _data;
    public double SpawnCost => _data != null ? _data.SpawnCost : double.MaxValue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _repository = new LocalMonsterRepository(AccountManager.Instance.Email);
        _collection = new MonsterCollection(_data.Tiers.Length, _data.MaxMonstersPerTier, MonstersRequiredForMerge);
        Load();
    }

    public double GetMergeCost(int tier)
    {
        if (_data.MergeCosts == null || tier < 0 || tier >= _data.MergeCosts.Length)
            return double.MaxValue;
        return _data.MergeCosts[tier];
    }

    public int GetMergeableTier() => _collection.FindMergeableTier();

    public int GetTierCount(int tier) => _collection.GetTierCount(tier);

    public bool CanSpawn()
    {
        return _data != null
            && _collection.CanAddMonster(0)
            && CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, _data.SpawnCost);
    }

    public bool TrySpawn()
    {
        if (!CanSpawn()) return false;

        CurrencyManager.Instance.Spend(ECurrencyType.Gold, _data.SpawnCost);
        _collection.AddMonster(0);
        Save();
        OnDataChanged?.Invoke();
        return true;
    }


    public bool CanMerge()
    {
        if (_data == null) return false;
        int tier = _collection.FindMergeableTier();
        if (tier < 0) return false;
        return CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, GetMergeCost(tier));
    }

    public bool TryMerge(out int sourceTier)
    {
        sourceTier = _collection.FindMergeableTier();
        if (sourceTier < 0) return false;

        double cost = GetMergeCost(sourceTier);
        if (!CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, cost)) return false;

        CurrencyManager.Instance.Spend(ECurrencyType.Gold, cost);

        for (int i = 0; i < MonstersRequiredForMerge; i++)
            _collection.RemoveMonster(sourceTier);
        _collection.AddMonster(sourceTier + 1);

        Save();
        OnDataChanged?.Invoke();
        return true;
    }

    public void NotifyMonsterRemoved(int tier)
    {
        _collection.RemoveMonster(tier);
        Save();
        OnDataChanged?.Invoke();
    }

    private void Save()
    {
        if (_data == null) return;
        _repository.Save(_collection.ToSaveData());
    }

    private void Load()
    {
        var saveData = _repository.Load();
        _collection.LoadFrom(saveData);
        OnDataChanged?.Invoke();
    }
}
