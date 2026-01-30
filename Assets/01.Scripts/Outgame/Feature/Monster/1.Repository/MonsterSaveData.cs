public class MonsterSaveData
{
    public int[] TierCounts;

    public static MonsterSaveData Empty => new MonsterSaveData
    {
        TierCounts = System.Array.Empty<int>()
    };
}
