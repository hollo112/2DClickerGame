using Cysharp.Threading.Tasks;

public interface ICurrencyRepository
{
    public UniTask Save(CurrencySaveData currencySaveData);
    public UniTask<CurrencySaveData> Load();
}
