using UnityEngine;

public class LocalMonsterRepository : IMonsterRepository
{
    private const string SaveKey = "Monster_Tier_";
    private const string TierCountKey = "Monster_TierCount";
    private readonly string _userId;

    public LocalMonsterRepository(string userId)
    {
        _userId = userId;
    }

    public void Save(MonsterSaveData saveData)
    {
        PlayerPrefs.SetInt(_userId + TierCountKey, saveData.TierCounts.Length);
        for (int i = 0; i < saveData.TierCounts.Length; i++)
        {
            PlayerPrefs.SetInt(_userId + SaveKey + i, saveData.TierCounts[i]);
        }
        PlayerPrefs.Save();
    }

    public MonsterSaveData Load()
    {
        int tierCount = PlayerPrefs.GetInt(_userId + TierCountKey, 0);
        if (tierCount <= 0) return MonsterSaveData.Empty;

        var data = new MonsterSaveData
        {
            TierCounts = new int[tierCount]
        };
        for (int i = 0; i < tierCount; i++)
        {
            data.TierCounts[i] = PlayerPrefs.GetInt(_userId + SaveKey + i, 0);
        }
        return data;
    }
}
