using Cysharp.Threading.Tasks;

public interface IMonsterRepository
{
    public UniTask Save(MonsterSaveData saveData);
    public UniTask<MonsterSaveData> Load();
}
