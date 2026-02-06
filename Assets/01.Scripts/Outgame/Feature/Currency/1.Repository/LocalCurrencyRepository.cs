using Cysharp.Threading.Tasks;
using UnityEngine;

public class LocalCurrencyRepository : ICurrencyRepository
{
    private const string SaveKey = "Currency_";
    private const string TimestampKey = "Currency_Timestamp";
    private readonly string _userId;

    public LocalCurrencyRepository(string userId)
    {
        _userId = userId;
    }

    public async UniTask Save(CurrencySaveData saveData)
    {
        // 타임스탬프 업데이트
        saveData.UpdateTimestamp();

        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            var type = (ECurrencyType)i;
            PlayerPrefs.SetString(_userId + SaveKey + type.ToString(), saveData.Currencies[i].ToString("G17"));
        }
        // 타임스탬프 저장
        PlayerPrefs.SetString(_userId + TimestampKey, saveData.Timestamp.ToString());
        PlayerPrefs.Save();
        await UniTask.CompletedTask;
    }

    public async UniTask<CurrencySaveData> Load()
    {
        var data = new CurrencySaveData
        {
            Currencies = new double[(int)ECurrencyType.Count]
        };

        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            var type = (ECurrencyType)i;
            string key = _userId + SaveKey + type.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                data.Currencies[i] = double.Parse(PlayerPrefs.GetString(key, "0"));
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