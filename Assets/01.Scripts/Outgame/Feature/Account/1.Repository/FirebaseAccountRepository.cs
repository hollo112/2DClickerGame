using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseAccountRepository : IAccountRepository
{
    private FirebaseAuth _auth = FirebaseAuth.DefaultInstance;
    
    public async UniTask<AccountResult> Register(string email, string password)
    {
        try
        {
            await _auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
            return new AccountResult()
            {
                Success = true,
            };
        }
        catch (Exception e)
        {
            return new AccountResult()
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
       
    }

    public async UniTask<AccountResult> Login(string email, string password)
    {
        try
        {
            await _auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
            return new AccountResult()
            {
                Success = true,
                Account = new Account(email, password),
            };
        }
        catch (Exception e)
        {
            return new AccountResult()
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public void Logout()
    {
        _auth.SignOut();
    }
}