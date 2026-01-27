public class AutoClickUpgradeButton : UpgradeButtonBase
{
    private bool IsUnlocked => UpgradeManager.Instance.IsAutoClickUnlocked;

    protected override bool TryUpgrade()
    {
        var type = IsUnlocked ? UpgradeType.AutoClickSpeed : UpgradeType.AutoClickUnlock;
        return UpgradeManager.Instance.TryUpgrade(type);
    }

    protected override void UpdateDisplay()
    {
        var upgrade = UpgradeManager.Instance;

        if (!IsUnlocked)
        {
            SetDisplay(
                "오토클릭",
                "클릭 유지 시 자동 채굴",
                "미해금",
                $"{GetCurrentCost().ToFormattedString()} G"
            );
            return;
        }

        int level = upgrade.GetCurrentLevel(UpgradeType.AutoClickSpeed);
        double cost = GetCurrentCost();
        bool isMax = upgrade.IsMaxLevel(UpgradeType.AutoClickSpeed);

        SetDisplay(
            "오토클릭",
            "자동 채굴 속도 증가",
            $"Lv.{level}",
            isMax ? "MAX" : $"{cost.ToFormattedString()} G"
        );
    }

    protected override double GetCurrentCost()
    {
        var type = IsUnlocked ? UpgradeType.AutoClickSpeed : UpgradeType.AutoClickUnlock;
        return UpgradeManager.Instance.GetUpgradeCost(type);
    }
}