using System.Collections;

namespace WARSApplication.Data.SharedDataProperties;

public interface IAddOnlyCollection<T> : IEnumerable<T>
{
    public void Add(T item);
}

public class AddOnlyCollection<T>(IEnumerable<T>? initialItems = null) : IAddOnlyCollection<T>
{
    private readonly List<T> _items = [..initialItems ?? []];
    public void Add(T item) => _items.Add(item);
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}