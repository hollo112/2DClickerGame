using System;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseMonsterRepository : IMonsterRepository
{
    private readonly string _userId;
    private FirebaseFirestore _db = FirebaseFirestore.DefaultInstance;

    public FirebaseMonsterRepository(string userId)
    {
        _userId = userId;
    }

    private DocumentReference GetDocument()
    {
        return _db.Collection("users").Document(_userId).Collection("monster").Document("data");
    }

    public async UniTask Save(MonsterSaveData saveData)
    {
        try
        {
            // 타임스탬프 업데이트
            saveData.UpdateTimestamp();
            await GetDocument().SetAsync(saveData).AsUniTask();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseMonsterRepository] Save 실패: {e.Message}");
        }
    }

    public async UniTask<MonsterSaveData> Load()
    {
        try
        {
            var snapshot = await GetDocument().GetSnapshotAsync().AsUniTask();

            if (!snapshot.Exists)
                return MonsterSaveData.Empty;

            return snapshot.ConvertTo<MonsterSaveData>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseMonsterRepository] Load 실패: {e.Message}");
            return MonsterSaveData.Empty;
        }
    }
}
