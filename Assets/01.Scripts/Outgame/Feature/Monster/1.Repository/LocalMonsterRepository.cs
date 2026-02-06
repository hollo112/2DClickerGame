using Cysharp.Threading.Tasks;
using UnityEngine;

public class LocalMonsterRepository : IMonsterRepository
{
    private const string SaveKey = "Monster_Tier_";
    private const string TierCountKey = "Monster_TierCount";
    private const string TimestampKey = "Monster_Timestamp";
    private readonly string _userId;

    public LocalMonsterRepository(string userId)
    {
        _userId = userId;
    }

    public async UniTask Save(MonsterSaveData saveData)
    {
        // 타임스탬프 업데이트
        saveData.UpdateTimestamp();

        PlayerPrefs.SetInt(_userId + TierCountKey, saveData.TierCounts.Length);
        for (int i = 0; i < saveData.TierCounts.Length; i++)
        {
            PlayerPrefs.SetInt(_userId + SaveKey + i, saveData.TierCounts[i]);
        }
        // 타임스탬프 저장
        PlayerPrefs.SetString(_userId + TimestampKey, saveData.Timestamp.ToString());
        PlayerPrefs.Save();
        await UniTask.CompletedTask;
    }

    public async UniTask<MonsterSaveData> Load()
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
        // 타임스탬프 로드
        if (long.TryParse(PlayerPrefs.GetString(_userId + TimestampKey, "0"), out long timestamp))
        {
            data.Timestamp = timestamp;
        }
        return data;
    }
}
