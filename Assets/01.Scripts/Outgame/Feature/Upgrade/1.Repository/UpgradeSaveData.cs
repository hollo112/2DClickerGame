using System;
using Firebase.Firestore;   

[Serializable]
[FirestoreData]
public class UpgradeSaveData
{
    [FirestoreProperty]
    public int[] Levels {get; set;}

    public static UpgradeSaveData Default => new UpgradeSaveData
    {
        Levels = new int[(int)EUpgradeType.Count]
    };
}
