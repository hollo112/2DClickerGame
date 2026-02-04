using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [SerializeField] private UpgradeSpecTableSO _specTable;
    private Dictionary<EUpgradeType, Upgrade> _upgrades = new();

    private IUpgradeRepository _repository;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _repository = new FirebaseUpgradeRepository(AccountManager.Instance.Email);
        InitializeUpgrades();
    }

    private void Start()
    {
        Load();
    }

    private void InitializeUpgrades()
    {
        if (_specTable == null || _specTable.Datas == null) return;

        foreach (var spec in _specTable.Datas)
        {
            if (spec == null) continue;
            _upgrades[spec.Type] = new Upgrade(spec);
        }
    }

    public Upgrade Get(EUpgradeType type) => _upgrades.TryGetValue(type, out var u) ? u : null;
    public List<Upgrade> GetAll() => _upgrades.Values.ToList();

    public bool CanLevelUp(EUpgradeType type)
    {
        var upgrade = Get(type);
        if (upgrade == null) return false;
        if (!upgrade.CanLevelUp()) return false;
        return CurrencyManager.Instance.CanAfford(ECurrencyType.Gold, upgrade.Cost);
    }

    public bool TryLevelUp(EUpgradeType type)
    {
        var upgrade = Get(type);
        if (upgrade == null) return false;

        if (!upgrade.CanLevelUp())
        {
            Debug.Log($"[Upgrade] {type} 실패 - 최대 레벨");
            return false;
        }

        double cost = upgrade.Cost;
        if (!CurrencyManager.Instance.Spend(ECurrencyType.Gold, cost))
        {
            Debug.Log($"[Upgrade] {type} 실패 - 비용 부족");
            return false;
        }

        upgrade.TryLevelUp();
        Debug.Log($"[Upgrade] {type} 레벨 UP! Lv.{upgrade.Level}");

        Save();
        OnDataChanged?.Invoke();
        return true;
    }

    private void Save()
    {
        var saveData = new UpgradeSaveData
        {
            Levels = new int[(int)EUpgradeType.Count]
        };
        for (int i = 0; i < (int)EUpgradeType.Count; i++)
        {
            var type = (EUpgradeType)i;
            var upgrade = Get(type);
            saveData.Levels[i] = upgrade?.Level ?? 0;
        }
        _repository.Save(saveData).Forget();
    }

    private async void Load()
    {
        var saveData = await _repository.Load();
        for (int i = 0; i < (int)EUpgradeType.Count; i++)
        {
            if (saveData.Levels[i] > 0)
            {
                var type = (EUpgradeType)i;
                var upgrade = Get(type);
                upgrade?.SetLevel(saveData.Levels[i]);
            }
        }
        OnDataChanged?.Invoke();
    }
}
