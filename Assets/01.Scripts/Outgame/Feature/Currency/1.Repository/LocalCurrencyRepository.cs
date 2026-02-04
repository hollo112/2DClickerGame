using Cysharp.Threading.Tasks;
using UnityEngine;

public class LocalCurrencyRepository : ICurrencyRepository 
{
    private const string SaveKey = "Currency_";
    private readonly string _userId;

    public LocalCurrencyRepository(string userId)
    {
        _userId = userId;
    }
    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            var type = (ECurrencyType)i;
            PlayerPrefs.SetString(_userId + SaveKey + type.ToString(), saveData.Currencies[i].ToString("G17"));
        }
        PlayerPrefs.Save();
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

        return data;
    }
}