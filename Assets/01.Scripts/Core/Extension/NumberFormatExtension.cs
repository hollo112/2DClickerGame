public static class NumberFormatExtension 
{
    // 확장 메서드
    // 이미 존재하는 클래스에 메서드를 추가하는 기능
    private static string[] _suffixes =
    {
        "", "K", "M", "B", "T",
        "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj",
        "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at",
        "au", "av", "aw", "ax", "ay", "az",
        "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj",
        "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt",
        "bu", "bv", "bw", "bx", "by", "bz"
    };
    
    public static string ToFormattedString(this double number)
    {
        if (number < 1000) return number.ToString("N0");

        int suffixIndex = 0;

        double value = number;
        while (value >= 1000 && suffixIndex < _suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }
        
        string numberStr;
        if (value >= 100)
            numberStr = $"{value:F0}";
        else if (value >= 10)
            numberStr = $"{value:F1}".TrimEnd('0').TrimEnd('.');
        else
            numberStr = $"{value:F2}".TrimEnd('0').TrimEnd('.');

        return $"{numberStr}{_suffixes[suffixIndex]}";
    }
}
