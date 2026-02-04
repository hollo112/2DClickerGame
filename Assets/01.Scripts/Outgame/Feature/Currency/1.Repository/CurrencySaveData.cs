using System;
using Firebase.Firestore;
using UnityEngine;

[Serializable]
[FirestoreData]
public class CurrencySaveData
{
    [FirestoreProperty]
    public double[] Currencies {get; set;}

    public static CurrencySaveData Default => new CurrencySaveData
    {
        Currencies = new double[(int)ECurrencyType.Count]
    };
}