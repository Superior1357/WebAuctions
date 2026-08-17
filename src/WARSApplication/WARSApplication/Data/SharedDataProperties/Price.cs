using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WARSApplication.Data.SharedDataProperties;

public readonly record struct Price(decimal Value)
{
    public static implicit operator Price(decimal value) => new(value);
}

public class PriceConverter : ValueConverter<Price, decimal>
{
    public PriceConverter() : base(p => p.Value, v => new Price(v)) { }
}