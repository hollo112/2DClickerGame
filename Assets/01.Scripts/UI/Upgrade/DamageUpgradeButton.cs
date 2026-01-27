public class DamageUpgradeButton : UpgradeButtonBase
{
    protected override bool TryUpgrade()
    {
        return UpgradeManager.Instance.TryUpgrade(UpgradeType.Damage);
    }

    protected override void UpdateDisplay()
    {
        var upgrade = UpgradeManager.Instance;
        int level = upgrade.GetCurrentLevel(UpgradeType.Damage);
        double cost = GetCurrentCost();
        bool isMax = upgrade.IsMaxLevel(UpgradeType.Damage);

        SetDisplay(
            "약탈량",
            "클릭당 획득 재화 증가",
            $"Lv.{level}",
            isMax ? "MAX" : $"{cost:N0} G"
        );
    }

    protected override double GetCurrentCost()
    {
        return UpgradeManager.Instance.GetUpgradeCost(UpgradeType.Damage);
    }
}