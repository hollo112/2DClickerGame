using UnityEngine;

public class MonsterSpawnButton : UpgradeButtonBase
{
    private bool _subscribedToMonsterManager;

    protected override void Start()
    {
        base.Start();
        TrySubscribeToMonsterManager();
    }

    private void Update()
    {
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
        return MonsterManager.Instance.TrySpawnMonster();
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
        if (MonsterManager.Instance == null || MonsterManager.Instance.Data == null)
            return -1;
        return MonsterManager.Instance.Data.SpawnCost;
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