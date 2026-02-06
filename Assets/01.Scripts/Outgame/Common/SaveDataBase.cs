using System;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public abstract class SaveDataBase
{
    [FirestoreProperty]
    public long Timestamp { get; set; }

    public void UpdateTimestamp()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public bool IsNewerThan(SaveDataBase other)
    {
        if (other == null) return true;
        return Timestamp > other.Timestamp;
    }

    public static T GetNewer<T>(T a, T b) where T : SaveDataBase
    {
        if (a == null) return b;
        if (b == null) return a;
        return a.IsNewerThan(b) ? a : b;
    }
}