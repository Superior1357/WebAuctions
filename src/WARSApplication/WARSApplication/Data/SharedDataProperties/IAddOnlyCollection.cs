namespace WARSApplication.Data.SharedDataProperties;

public interface IAddOnlyCollection<T>
{
    public void Add(T item);
    public IEnumerable<T> GetItems();
}

public class AddOnlyCollection<T>(IEnumerable<T>? initialItems = null) : IAddOnlyCollection<T>
{
    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetItems()
    {
        throw new NotImplementedException();
    }
}