namespace WARSApplication.Data.SharedDataProperties;

public interface IAddOnlyCollection<T>
{
    public void Add(T item);
    public IEnumerable<T> GetItems();
}

public class AddOnlyCollection<T>(IEnumerable<T>? initialItems = null) : IAddOnlyCollection<T>
{
    private readonly List<T> _items = [..initialItems ?? []];
    public void Add(T item) => _items.Add(item);

    public IEnumerable<T> GetItems() => _items;
}