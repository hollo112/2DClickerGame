using UnityEngine;

public class MonsterMergeButton : UpgradeButtonBase
{
    private bool _subscribedToInGame;
    private bool _subscribedToOutGame;

    protected override void Start()
    {
        base.Start();
        TrySubscribe();
    }

    private void Update()
    {
        if (!_subscribedToInGame || !_subscribedToOutGame)
        {
            TrySubscribe();
        }
    }

    private void TrySubscribe()
    {
        if (MonsterInGameManager.Instance != null && !_subscribedToInGame)
        {
            MonsterInGameManager.Instance.OnMonsterChanged += Refresh;
            _subscribedToInGame = true;
        }
        if (!_subscribedToOutGame)
        {
            MonsterManager.OnDataChanged += Refresh;
            _subscribedToOutGame = true;
        }
        if (_subscribedToInGame && _subscribedToOutGame) Refresh();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (MonsterInGameManager.Instance != null && _subscribedToInGame)
        {
            MonsterInGameManager.Instance.OnMonsterChanged -= Refresh;
        }
        if (_subscribedToOutGame)
        {
            MonsterManager.OnDataChanged -= Refresh;
        }
    }

    protected override bool TryUpgrade()
    {
        if (MonsterInGameManager.Instance == null) return false;
        return MonsterInGameManager.Instance.TryMerge();
    }

    protected override void UpdateDisplay()
    {
        Currency cost = GetCurrentCost();
        int tier = MonsterManager.Instance != null ? MonsterManager.Instance.GetMergeableTier() : -1;
        string tierText = tier >= 0 ? $"\n      {tier + 1}에서 {tier + 2}단계" : "";

        SetDisplay(
            "몬스터 머지",
            $"같은 단계 3마리 합성{tierText}",
            "",
            cost >= 0 ? cost.ToString() : "-"
        );
    }

    protected override double GetCurrentCost()
    {
        if (MonsterManager.Instance == null) return -1;

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

        _button.interactable = MonsterManager.Instance.CanMerge();
    }
}
