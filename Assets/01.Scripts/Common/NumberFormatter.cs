public static class NumberFormatter
{
    private static readonly string[] Suffixes =
    {
        "", "K", "M", "B", "T",
        "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj",
        "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at",
        "au", "av", "aw", "ax", "ay", "az",
        "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj",
        "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt",
        "bu", "bv", "bw", "bx", "by", "bz"
    };

    public static string Format(double value)
    {
        if (value < 1000) return value.ToString("N0");

        int suffixIndex = 0;
        double displayValue = value;

        while (displayValue >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            displayValue /= 1000;
            suffixIndex++;
        }

        if (displayValue >= 100) return $"{displayValue:F0}{Suffixes[suffixIndex]}";
        if (displayValue >= 10) return $"{displayValue:F1}{Suffixes[suffixIndex]}";
        return $"{displayValue:F2}{Suffixes[suffixIndex]}";
    }
}
