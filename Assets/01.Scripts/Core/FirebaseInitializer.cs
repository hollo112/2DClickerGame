#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance{get; private set;}

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        InitFirebase().Forget();
    }

    private async UniTask InitFirebase()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();

        try
        {
            if (status == DependencyStatus.Available)
            {
                Debug.Log("Firebase 초기화 성공");
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Firebase 초기화 실패: {status}");
        }
        catch (Exception e)
        {
            Debug.LogError($"실패: {status}");
        }
    }
}
#endif