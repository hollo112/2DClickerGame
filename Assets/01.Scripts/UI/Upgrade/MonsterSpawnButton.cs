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
            MonsterOutgameManager.OnDataChanged += Refresh;
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
            MonsterOutgameManager.OnDataChanged -= Refresh;
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
        if (MonsterOutgameManager.Instance == null) return -1;
        return MonsterOutgameManager.Instance.SpawnCost;
    }

    protected override void UpdateInteractable()
    {
        if (MonsterOutgameManager.Instance == null)
        {
            _button.interactable = false;
            return;
        }

        _button.interactable = MonsterOutgameManager.Instance.CanSpawn();
    }
}
