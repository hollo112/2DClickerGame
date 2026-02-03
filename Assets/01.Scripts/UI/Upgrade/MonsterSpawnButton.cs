using UnityEngine;

public class MonsterSpawnButton : UpgradeButtonBase
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
        return MonsterInGameManager.Instance.TrySpawnMonster();
    }

    protected override void UpdateDisplay()
    {
        Currency cost = GetCurrentCost();

        SetDisplay(
            "몬스터 소환",
            "1단계 몬스터 소환",
            "",
            cost >= 0 ? cost.ToString() : "-"
        );
    }

    protected override double GetCurrentCost()
    {
        if (MonsterManager.Instance == null) return -1;
        return MonsterManager.Instance.SpawnCost;
    }

    protected override void UpdateInteractable()
    {
        if (MonsterManager.Instance == null)
        {
            _button.interactable = false;
            return;
        }

        _button.interactable = MonsterManager.Instance.CanSpawn();
    }
}
