public class FirebaseCurrencyRepository : ICurrencyRepository
{
    public void Save(CurrencySaveData currencySaveData)
    {
        
    }

    public CurrencySaveData Load()
    {
        return CurrencySaveData.Default;
    }
}