using System.Collections;

namespace WARSApplication.Data.SharedDataProperties;
/// <summary>
/// A collection that can only be modified by adding items.
/// </summary>
/// <typeparam name="T">item type</typeparam>
public interface IAddOnlyCollection<T> : IEnumerable<T>
{
    public void Add(T item);
}

/// <summary>
/// A collection that can only be modified by adding items.
/// </summary>
/// <param name="initialItems">items that are present in the collection from the beginning</param>
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