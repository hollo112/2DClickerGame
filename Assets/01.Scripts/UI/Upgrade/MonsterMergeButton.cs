using UnityEngine;

public class MonsterMergeButton : UpgradeButtonBase
{
    private bool _subscribedToMonsterManager;

    protected override void Start()
    {
        base.Start();
        TrySubscribeToMonsterManager();
    }

    private void Update()
    {
        // MonsterManager가 늦게 초기화될 경우를 대비
        if (!_subscribedToMonsterManager)
        {
            TrySubscribeToMonsterManager();
        }
    }

    private void TrySubscribeToMonsterManager()
    {
        if (MonsterManager.Instance != null && !_subscribedToMonsterManager)
        {
            MonsterManager.Instance.OnMonsterChanged += Refresh;
            _subscribedToMonsterManager = true;
            Refresh();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (MonsterManager.Instance != null && _subscribedToMonsterManager)
        {
            MonsterManager.Instance.OnMonsterChanged -= Refresh;
        }
    }

    protected override bool TryUpgrade()
    {
        if (MonsterManager.Instance == null) return false;
        return MonsterManager.Instance.TryMerge();
    }

    protected override void UpdateDisplay()
    {
        double cost = GetCurrentCost();
        int tier = MonsterManager.Instance != null ? MonsterManager.Instance.GetMergeableTier() : -1;
        string tierText = tier >= 0 ? $"\n      {tier + 1}에서 {tier + 2}단계" : "";

        SetDisplay(
            "몬스터 머지",
            $"같은 단계 3마리 합성{tierText}",
            "",
            cost >= 0 ? $"{cost.ToFormattedString()} G" : "-"
        );
    }

    protected override double GetCurrentCost()
    {
        if (MonsterManager.Instance == null || MonsterManager.Instance.Data == null)
            return -1;

        int tier = MonsterManager.Instance.GetMergeableTier();
        if (tier < 0) return -1;

        return MonsterManager.Instance.GetMergeCost(tier);
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
