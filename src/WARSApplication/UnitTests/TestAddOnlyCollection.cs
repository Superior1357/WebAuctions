using WARSApplication.Data.SharedDataProperties;

namespace UnitTests;

public sealed class TestAddOnlyCollection<T>(IEnumerable<T> items) : IAddOnlyCollection<T>
{
    private readonly List<T> _items = items.ToList();

    public void Add(T item) => _items.Add(item);

    public IEnumerable<T> GetItems() => _items;
}