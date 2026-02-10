using System;

public class Upgrade
{
    public readonly UpgradeSpecData SpecData;

    public int Level { get; private set; }
    public double Cost => SpecData.BaseCost * Math.Pow(SpecData.CostMultiplier, Math.Pow(Level, SpecData.CostExponent));
    public double Value => SpecData.BaseValue + Level * SpecData.ValuePerLevel;
    public bool IsMaxLevel => Level >= SpecData.MaxLevel;

    public Upgrade(UpgradeSpecData specData)
    {
        if (specData == null)
            throw new ArgumentNullException(nameof(specData));
        if (specData.MaxLevel < 1)
            throw new ArgumentException($"최대 레벨은 1 이상이어야 합니다: {specData.MaxLevel}");
        if (specData.BaseCost <= 0)
            throw new ArgumentException($"기본 비용은 0보다 커야 합니다: {specData.BaseCost}");
        if (specData.CostMultiplier <= 0)
            throw new ArgumentException($"비용 배율은 0보다 커야 합니다: {specData.CostMultiplier}");
        if (string.IsNullOrEmpty(specData.Name))
            throw new ArgumentException("이름은 비어있을 수 없습니다");
        if (string.IsNullOrEmpty(specData.Description))
            throw new ArgumentException("설명은 비어있을 수 없습니다");

        SpecData = specData;
        Level = 0;
    }

    public void SetLevel(int level)
    {
        Level = Math.Clamp(level, 0, SpecData.MaxLevel);
    }

    public bool CanLevelUp() => !IsMaxLevel;

    public bool TryLevelUp()
    {
        if (!CanLevelUp()) return false;
        Level++;
        return true;
    }
}
