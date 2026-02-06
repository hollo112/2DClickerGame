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

    public async UniTask Save(CurrencySaveData currencySaveData)
    {
        try
        {
            // 타임스탬프 업데이트
            currencySaveData.UpdateTimestamp();
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
