using Cysharp.Threading.Tasks;

public interface IMonsterRepository
{
    public UniTaskVoid Save(MonsterSaveData saveData);
    public UniTask<MonsterSaveData> Load();
}
