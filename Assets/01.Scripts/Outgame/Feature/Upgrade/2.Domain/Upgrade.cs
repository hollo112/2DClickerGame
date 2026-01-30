using System;

public class Upgrade
{
    public readonly UpgradeSpecData SpecData;

    public int Level { get; private set; }
    public double Cost => SpecData.BaseCost + Math.Pow(SpecData.CostMultiplier, Level);
    public double Value => SpecData.BaseValue + Level * SpecData.ValuePerLevel;
    public bool IsMaxLevel => Level >= SpecData.MaxLevel;

    public Upgrade(UpgradeSpecData specData)
    {
        if (specData == null)
            throw new ArgumentNullException(nameof(specData));

        SpecData = specData;
        Level = 0;
    }

    public bool CanLevelUp() => !IsMaxLevel;

    public bool TryLevelUp()
    {
        if (!CanLevelUp()) return false;
        Level++;
        return true;
    }
}
