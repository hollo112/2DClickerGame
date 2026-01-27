public class MonsterMergeButton : UpgradeButtonBase
{
    protected override void Start()
    {
        base.Start();
        if (MonsterManager.Instance != null)
            MonsterManager.Instance.OnMonsterChanged += Refresh;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (MonsterManager.Instance != null)
            MonsterManager.Instance.OnMonsterChanged -= Refresh;
    }

    protected override bool TryUpgrade()
    {
        if (MonsterManager.Instance == null) return false;
        return MonsterManager.Instance.TryMerge();
    }

    protected override void UpdateDisplay()
    {
        double cost = GetCurrentCost();

        SetDisplay(
            "몬스터 머지",
            "같은 단계 3마리 합성",
            "",
            $"{cost.ToFormattedString()} G"
        );
    }

    protected override double GetCurrentCost()
    {
        if (MonsterManager.Instance == null || MonsterManager.Instance.Data == null)
            return -1;
        return MonsterManager.Instance.Data.MergeCost;
    }

    protected override void UpdateInteractable()
    {
        if (MonsterManager.Instance == null)
        {
            _button.interactable = false;
            return;
        }

        // 머지 가능 조건: 비용 충족 && 같은 단계 3마리 이상 && 5단계 미만
        _button.interactable = MonsterManager.Instance.CanMerge();
    }
}
