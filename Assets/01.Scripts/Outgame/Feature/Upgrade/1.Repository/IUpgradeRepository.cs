using Cysharp.Threading.Tasks;

public interface IUpgradeRepository
{
    public UniTask Save(UpgradeSaveData saveData);
    public UniTask<UpgradeSaveData> Load();
}
