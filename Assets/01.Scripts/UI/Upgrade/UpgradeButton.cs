using UnityEngine;

public class UpgradeButton : UpgradeButtonBase
{
    [SerializeField] private EUpgradeType _type;

    protected override bool TryUpgrade()
    {
        return UpgradeManager.Instance.TryLevelUp(_type);
    }

    protected override void UpdateDisplay()
    {
        var upgrade = UpgradeManager.Instance.Get(_type);
        if (upgrade == null)
        {
            SetDisplay("???", "", "", "-");
            return;
        }

        SetDisplay(
            upgrade.SpecData.Name,
            upgrade.SpecData.Description,
            $"Lv.{upgrade.Level}",
            upgrade.IsMaxLevel ? "MAX" : new Currency(upgrade.Cost).ToString()
        );
    }

    protected override double GetCurrentCost()
    {
        var upgrade = UpgradeManager.Instance.Get(_type);
        return upgrade?.Cost ?? -1;
    }
}
