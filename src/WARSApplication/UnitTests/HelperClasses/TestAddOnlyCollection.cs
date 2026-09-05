using System.Collections;
using WARSApplication.Data.SharedDataProperties;

namespace UnitTests.HelperClasses;

public sealed class TestAddOnlyCollection<T>(IEnumerable<T> items) : IAddOnlyCollection<T>
{
    private readonly List<T> _items = items.ToList();

    public void Add(T item) => _items.Add(item);
    public int Count => _items.Count;
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}