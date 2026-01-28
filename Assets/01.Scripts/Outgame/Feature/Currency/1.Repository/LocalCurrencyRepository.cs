using UnityEngine;

public class LocalCurrencyRepository : ICurrencyRepository 
{
    private const string SaveKey = "Currency_";

    public void Save(CurrencySaveData saveData)
    {
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            var type = (ECurrencyType)i;
            PlayerPrefs.SetString(SaveKey + type.ToString(), saveData.Currencies[i].ToString("G17"));
        }
        PlayerPrefs.Save();
    }

    public CurrencySaveData Load()
    {
        var data = new CurrencySaveData
        {
            Currencies = new double[(int)ECurrencyType.Count]
        };

        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            var type = (ECurrencyType)i;
            string key = SaveKey + type.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                data.Currencies[i] = double.Parse(PlayerPrefs.GetString(key, "0"));
            }
        }

        return data;
    }
}