using UnityEngine;

public class LocalAccountRepository : IAccountRepository
{
    public void Save(AccountSaveData saveData)
    {
        PlayerPrefs.SetString(saveData.Email, saveData.Password);
        PlayerPrefs.Save();
    }

    public AccountSaveData Load(string email)
    {
        if (!PlayerPrefs.HasKey(email)) return null;

        return new AccountSaveData
        {
            Email = email,
            Password = PlayerPrefs.GetString(email)
        };
    }

    public bool Exists(string email)
    {
        return PlayerPrefs.HasKey(email);
    }
}
