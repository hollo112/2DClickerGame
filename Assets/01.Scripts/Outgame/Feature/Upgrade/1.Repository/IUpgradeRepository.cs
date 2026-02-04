using Cysharp.Threading.Tasks;

public interface IUpgradeRepository
{
    public UniTaskVoid Save(UpgradeSaveData saveData);
    public UniTask<UpgradeSaveData> Load();
}
