public class BuildingUpgradeButton : UpgradeButtonBase
{
    private bool IsUnlocked => UpgradeManager.Instance.IsBuildingUnlocked;

    protected override bool TryUpgrade()
    {
        var type = IsUnlocked ? UpgradeType.BuildingIncome : UpgradeType.BuildingUnlock;
        return UpgradeManager.Instance.TryUpgrade(type);
    }

    protected override void UpdateDisplay()
    {
        var upgrade = UpgradeManager.Instance;

        if (!IsUnlocked)
        {
            SetDisplay(
                "건축물",
                "시간당 자동 수입 생성",
                "미해금",
                $"{GetCurrentCost():N0} G"
            );
            return;
        }

        int level = upgrade.GetCurrentLevel(UpgradeType.BuildingIncome);
        double cost = GetCurrentCost();
        bool isMax = upgrade.IsMaxLevel(UpgradeType.BuildingIncome);

        SetDisplay(
            "건축물",
            "수입 속도 증가",
            $"Lv.{level}",
            isMax ? "MAX" : $"{cost:N0} G"
        );
    }

    protected override double GetCurrentCost()
    {
        var type = IsUnlocked ? UpgradeType.BuildingIncome : UpgradeType.BuildingUnlock;
        return UpgradeManager.Instance.GetUpgradeCost(type);
    }
}
