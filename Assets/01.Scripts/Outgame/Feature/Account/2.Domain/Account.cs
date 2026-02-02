using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Account
{
    public readonly string Email;
    public readonly string Password;
    
    public Account(string email, string password)
    {
        var emailSpec = new AccountEmailSpecification();
        if (!emailSpec.IsStatisfiedBy(email))
        {
            throw new ArgumentException(emailSpec.ErrorMessage);
        }
        if (string.IsNullOrEmpty(password)) throw new ArgumentException($"비밀번호는 비어있을 수 없습니다.");
        if (password.Length < 6 || 15 < password.Length) throw new ArgumentException($"비밀번호는 6~16자 사이어야합니다");
        
        Email = email;
        Password = password;
    }
}
