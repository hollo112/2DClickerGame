public class UpgradeSaveData
{
    public int[] Levels;

    public static UpgradeSaveData Default => new UpgradeSaveData
    {
        Levels = new int[(int)EUpgradeType.Count]
    };
}
