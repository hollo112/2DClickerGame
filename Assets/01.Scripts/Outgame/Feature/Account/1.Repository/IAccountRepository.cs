public interface IAccountRepository
{
    void Save(AccountSaveData saveData);
    AccountSaveData Load(string email);
    bool Exists(string email);
}
