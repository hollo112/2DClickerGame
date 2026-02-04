using System;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class MonsterSaveData
{
    [FirestoreProperty]
    public int[] TierCounts {get; set;}

    public static MonsterSaveData Empty => new MonsterSaveData
    {
        TierCounts = Array.Empty<int>()
    };
}
