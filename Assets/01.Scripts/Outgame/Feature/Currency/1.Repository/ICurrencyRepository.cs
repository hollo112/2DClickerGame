using Cysharp.Threading.Tasks;

public interface ICurrencyRepository 
{
    public UniTaskVoid Save(CurrencySaveData currencySaveData);
    public UniTask<CurrencySaveData> Load();
}
