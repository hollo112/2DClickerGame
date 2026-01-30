using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [SerializeField] private UpgradeSpecTableSO _specTable;
    private Dictionary<EUpgradeType, Upgrade> _upgrades = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeUpgrades();
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

        OnDataChanged?.Invoke();
        return true;
    }
}
