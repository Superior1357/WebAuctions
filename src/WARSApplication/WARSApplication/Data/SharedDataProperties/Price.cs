using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WARSApplication.Data.SharedDataProperties;

public readonly record struct Price(decimal Value)
{
    public static implicit operator Price(decimal value) => new(value);
    public static Price operator* (int times, Price price) => new(price.Value * times);
    public static Price operator- (Price divisor, Price dividend) => new(divisor.Value - dividend.Value);
}

public class PriceConverter : ValueConverter<Price, decimal>
{
    public PriceConverter() : base(p => p.Value, v => new Price(v)) { }
}