public class CurrencySaveData
{
    public double[] Currencies;

    public static CurrencySaveData Default => new CurrencySaveData
    {
        Currencies = new double[(int)ECurrencyType.Count]
    };
}