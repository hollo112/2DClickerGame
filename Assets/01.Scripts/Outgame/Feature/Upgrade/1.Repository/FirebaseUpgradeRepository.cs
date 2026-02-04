using System;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseUpgradeRepository : IUpgradeRepository
{
    private readonly string _userId;
    private FirebaseFirestore _db = FirebaseFirestore.DefaultInstance;

    public FirebaseUpgradeRepository(string userId)
    {
        _userId = userId;
    }

    private DocumentReference GetDocument()
    {
        return _db.Collection("users").Document(_userId).Collection("upgrade").Document("data");
    }

    public async UniTaskVoid Save(UpgradeSaveData saveData)
    {
        try
        {
            await GetDocument().SetAsync(saveData).AsUniTask();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseUpgradeRepository] Save 실패: {e.Message}");
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        try
        {
            var snapshot = await GetDocument().GetSnapshotAsync().AsUniTask();

            if (!snapshot.Exists)
                return UpgradeSaveData.Default;

            return snapshot.ConvertTo<UpgradeSaveData>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseUpgradeRepository] Load 실패: {e.Message}");
            return UpgradeSaveData.Default;
        }
    }
}
