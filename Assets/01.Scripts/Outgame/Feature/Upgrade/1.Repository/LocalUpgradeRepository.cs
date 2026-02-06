using Cysharp.Threading.Tasks;
using UnityEngine;

public class LocalUpgradeRepository : IUpgradeRepository
{
    private const string SaveKey = "Upgrade_";
    private const string TimestampKey = "Upgrade_Timestamp";
    private readonly string _userId;

    public LocalUpgradeRepository(string userId)
    {
        _userId = userId;
    }

    public async UniTask Save(UpgradeSaveData saveData)
    {
        // 타임스탬프 업데이트
        saveData.UpdateTimestamp();

        for (int i = 0; i < (int)EUpgradeType.Count; i++)
        {
            var type = (EUpgradeType)i;
            PlayerPrefs.SetInt(_userId + SaveKey + type.ToString(), saveData.Levels[i]);
        }
        // 타임스탬프 저장
        PlayerPrefs.SetString(_userId + TimestampKey, saveData.Timestamp.ToString());
        PlayerPrefs.Save();
        await UniTask.CompletedTask;
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        var data = new UpgradeSaveData
        {
            Levels = new int[(int)EUpgradeType.Count]
        };

        for (int i = 0; i < (int)EUpgradeType.Count; i++)
        {
            var type = (EUpgradeType)i;
            string key = _userId + SaveKey + type.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                data.Levels[i] = PlayerPrefs.GetInt(key, 0);
            }
        }

        // 타임스탬프 로드
        if (long.TryParse(PlayerPrefs.GetString(_userId + TimestampKey, "0"), out long timestamp))
        {
            data.Timestamp = timestamp;
        }

        return data;
    }
}
