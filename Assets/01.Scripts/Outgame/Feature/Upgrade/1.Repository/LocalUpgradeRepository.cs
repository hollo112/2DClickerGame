using UnityEngine;

public class LocalUpgradeRepository : IUpgradeRepository
{
    private const string SaveKey = "Upgrade_";
    private readonly string _userId;

    public LocalUpgradeRepository(string userId)
    {
        _userId = userId;
    }

    public void Save(UpgradeSaveData saveData)
    {
        for (int i = 0; i < (int)EUpgradeType.Count; i++)
        {
            var type = (EUpgradeType)i;
            PlayerPrefs.SetInt(_userId + SaveKey + type.ToString(), saveData.Levels[i]);
        }
        PlayerPrefs.Save();
    }

    public UpgradeSaveData Load()
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

        return data;
    }
}
