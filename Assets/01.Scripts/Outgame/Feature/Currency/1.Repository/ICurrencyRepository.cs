public interface ICurrencyRepository 
{
    public void Save(CurrencySaveData currencySaveData);
    public CurrencySaveData Load();
}
