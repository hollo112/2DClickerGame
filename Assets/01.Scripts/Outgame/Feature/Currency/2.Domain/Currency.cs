using System;

public struct Currency
{
    public double Value;

    public Currency(double value)
    {
        if (value < 0)
        {
            throw new Exception("Value cannot be negative");
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value.ToFormattedString();
    }

    // 암시적 변환
    public static implicit operator Currency(double value) => new Currency(Math.Max(0, value));
    public static implicit operator Currency(int value) => new Currency(Math.Max(0, value));
    public static implicit operator double(Currency currency) => currency.Value;

    // Currency + Currency
    public static Currency operator +(Currency a, Currency b) => new Currency(a.Value + b.Value);
    public static Currency operator -(Currency a, Currency b) => new Currency(Math.Max(0, a.Value - b.Value));

    // Currency 비교
    public static bool operator >(Currency a, Currency b) => a.Value > b.Value;
    public static bool operator <(Currency a, Currency b) => a.Value < b.Value;
    public static bool operator >=(Currency a, Currency b) => a.Value >= b.Value;
    public static bool operator <=(Currency a, Currency b) => a.Value <= b.Value;
    public static bool operator ==(Currency a, Currency b) => Math.Abs(a.Value - b.Value) < 0.0001;
    public static bool operator !=(Currency a, Currency b) => !(a == b);

    // double 비교
    public static bool operator >(Currency a, double b) => a.Value > b;
    public static bool operator <(Currency a, double b) => a.Value < b;
    public static bool operator >=(Currency a, double b) => a.Value >= b;
    public static bool operator <=(Currency a, double b) => a.Value <= b;

    // int 비교
    public static bool operator >(Currency a, int b) => a.Value > b;
    public static bool operator <(Currency a, int b) => a.Value < b;
    public static bool operator >=(Currency a, int b) => a.Value >= b;
    public static bool operator <=(Currency a, int b) => a.Value <= b;

    public override bool Equals(object obj) => obj is Currency other && this == other;
    public override int GetHashCode() => Value.GetHashCode();
}