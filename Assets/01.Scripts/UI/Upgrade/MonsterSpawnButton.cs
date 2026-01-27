public class MonsterSpawnButton : UpgradeButtonBase
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
        return MonsterManager.Instance.TrySpawnMonster();
    }

    protected override void UpdateDisplay()
    {
        double cost = GetCurrentCost();

        SetDisplay(
            "몬스터 소환",
            "1단계 몬스터 소환",
            "",
            $"{cost.ToFormattedString()} G"
        );
    }

    protected override double GetCurrentCost()
    {
        if (MonsterManager.Instance == null || MonsterManager.Instance.Data == null)
            return -1;
        return MonsterManager.Instance.Data.SpawnCost;
    }
}
