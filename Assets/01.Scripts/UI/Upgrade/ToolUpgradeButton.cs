public class ToolUpgradeButton : UpgradeButtonBase
{
    protected override bool TryUpgrade()
    {
        return UpgradeManager.Instance.TryUpgrade(UpgradeType.Tool);
    }

    protected override void UpdateDisplay()
    {
        var upgrade = UpgradeManager.Instance;
        int level = upgrade.GetCurrentLevel(UpgradeType.Tool);
        double cost = GetCurrentCost();
        bool isMax = upgrade.IsMaxLevel(UpgradeType.Tool);

        SetDisplay(
            "총 강화",
            "더 높은 등급의 주민을 약탈",
            $"Lv.{level}",
            isMax ? "MAX" : $"{cost:N0} G"
        );
    }

    protected override double GetCurrentCost()
    {
        return UpgradeManager.Instance.GetUpgradeCost(UpgradeType.Tool);
    }
}