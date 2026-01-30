public interface IMonsterRepository
{
    public void Save(MonsterSaveData saveData);
    public MonsterSaveData Load();
}
