namespace WARSApplication.Data.SharedDataProperties;

public readonly record struct Price(decimal Value)
{
    public static implicit operator Price(decimal value) => new(value);
}