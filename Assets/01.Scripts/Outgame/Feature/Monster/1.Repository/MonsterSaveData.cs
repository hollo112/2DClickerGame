using System;

public class MonsterSaveData
{
    public int[] TierCounts;

    public static MonsterSaveData Empty => new MonsterSaveData
    {
        TierCounts = Array.Empty<int>()
    };
}
