public interface IUpgradeRepository
{
    public void Save(UpgradeSaveData saveData);
    public UpgradeSaveData Load();
}
