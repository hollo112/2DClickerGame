using System;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    private readonly string _userId;
    private FirebaseFirestore _db = FirebaseFirestore.DefaultInstance;

    public FirebaseCurrencyRepository(string userId)
    {
        _userId = userId;
    }

    private DocumentReference GetDocument()
    {
        return _db.Collection("users").Document(_userId).Collection("currency").Document("data");
    }

    public async UniTaskVoid Save(CurrencySaveData currencySaveData)
    {
        try
        {
            await GetDocument().SetAsync(currencySaveData).AsUniTask();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseCurrencyRepository] Save 실패: {e.Message}");
        }
    }

    public async UniTask<CurrencySaveData> Load()
    {
        try
        {
            var snapshot = await GetDocument().GetSnapshotAsync().AsUniTask();

            if (!snapshot.Exists)
                return CurrencySaveData.Default;

            return snapshot.ConvertTo<CurrencySaveData>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseCurrencyRepository] Load 실패: {e.Message}");
            return CurrencySaveData.Default;
        }
    }
}
