using System;

[Serializable]
public class UpgradeSpecData
{
    public EUpgradeType Type;
    public int MaxLevel;
    public double BaseCost;
    public double BaseValue;
    public double CostMultiplier;
    public double ValuePerLevel;
    public string Name;
    public string Description;
}
