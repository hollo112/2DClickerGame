using System;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class CurrencySaveData : SaveDataBase
{
    [FirestoreProperty]
    public double[] Currencies { get; set; }

    public static CurrencySaveData Default => new CurrencySaveData
    {
        Currencies = new double[(int)ECurrencyType.Count],
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };
}