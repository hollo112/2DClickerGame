using System;
using Cysharp.Threading.Tasks;
using Firebase;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;

public class FirebaseTutorial : MonoBehaviour
{
    private FirebaseApp _app = null;
    private FirebaseAuth _auth = null;
    private FirebaseFirestore _db = null;
    
    public TextMeshProUGUI _progressText;

    private async void Start()
    {
        // 1. 파이어베이스 초기화
        _progressText.SetText("InitFirebase Start");
        await InitFirebase();
        _progressText.SetText("InitFirebase End");
            
        // 2. 로그아웃
        _progressText.SetText("Logout Start");
        await Logout();
        _progressText.SetText("Logout End");
            
        // 3. 재로그인
        _progressText.SetText("LogIn Start");
        await Login("yj@naver.com", "123456");
        _progressText.SetText("LogIn End");
            
        // 4. 강아지 추가
        _progressText.SetText("SaveDog Start");
        await SaveDog();
        _progressText.SetText("SaveDog End");
    }
    
    private async UniTask InitFirebase()
    {
        var result = await FirebaseApp.CheckAndFixDependenciesAsync();
        
        if (result == DependencyStatus.Available)
        {
            _app = FirebaseApp.DefaultInstance;         // 파이어베이스 앱   모듈 가져오기
            _auth = FirebaseAuth.DefaultInstance;       // 파이어베이스 인증 모듈 가져오기
            _db = FirebaseFirestore.DefaultInstance;    // 파이어베이스 DB  모듈 가져오기
            Debug.Log("[Firebase Tutorial]: Started");
        }
        else
        {
            throw new Exception($"Firebase 초기화 실패: {result}");
        }
    }

    public async UniTask Register(string email, string password)
    {
        try
        {
            var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", 
                result.User.DisplayName, result.User.UserId);
        }
        catch (Exception e)
        {
            Debug.LogError($"CreateUserWithEmailAndPasswordAsync encountered an error: {e.Message}");
            throw;
        }
    }

    private async UniTask Login(string email, string password)
    {
        try
        {
            var result = await _auth.SignInWithEmailAndPasswordAsync(email, password);
            Debug.LogFormat("로그인 성공: {0} ({1})", result.User.Email, result.User.UserId);
        }
        catch (Exception e)
        {
            Debug.LogError($"SignInWithEmailAndPasswordAsync encountered an error: {e.Message}");
            throw;
        }
    }

    private async UniTask Logout()
    {
        await UniTask.Yield(); // 메인 스레드에서 실행 보장
        
        if (_auth != null)
        {
            _auth.SignOut();
            Debug.Log("로그아웃 완료");
        }
    }

    private void CheckLoginStatus()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null)
        {
            Debug.Log("로그인 안됨");
        }
        else
        {
            Debug.LogFormat("로그인 중: {0} ({1})", user.Email, user.UserId);
        }
    }

    private async UniTask SaveDog()
    {
        try
        {
            Dog dog = new Dog("까미", 5);
            await _db.Collection("Dogs").Document("yj@naver.com").SetAsync(dog);
            Debug.Log("강아지 저장 완료!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase Tutorial]: Failed to save dog: {e.Message}");
            throw;
        }
    }

    private async UniTask LoadDog()
    {
        try
        {
            var snapshot = await _db.Collection("Dogs").Document("yj@naver.com").GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                Dog myDog = snapshot.ConvertTo<Dog>();
                Debug.LogFormat($"강아지 로드 완료: {myDog.Name}, {myDog.Age}");
            }
            else
            {
                Debug.LogError("[Firebase Tutorial]: No Dog found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase Tutorial]: Failed to load dog: {e.Message}");
            throw;
        }
    }

    private async UniTask DeleteDog()
    {
        try
        {
            var querySnapshot = await _db.Collection("Dogs")
                .WhereEqualTo("Name", "까미")
                .GetSnapshotAsync();

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                Dog myDog = doc.ConvertTo<Dog>();
                if (myDog.Name == "까미")
                {
                    await _db.Collection("Dogs").Document(doc.Id).DeleteAsync();
                    Debug.Log($"강아지 삭제 완료: {doc.Id}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase Tutorial]: Failed to delete dog: {e.Message}");
            throw;
        }
    }
    
    private void Update()
    {
        if (_app == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Register("yj@naver.com", "123456").Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Login("yj@naver.com", "123456").Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Logout().Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CheckLoginStatus();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SaveDog().Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            LoadDog().Forget();
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            DeleteDog().Forget();
        }
    }
}