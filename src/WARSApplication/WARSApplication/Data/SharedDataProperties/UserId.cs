namespace WARSApplication.Data.SharedDataProperties;

public readonly record struct UserId(int Id)
{
    public static UserId operator+(UserId id, int n) => new (id.Id + n);
    
    public static bool TryParse(string id, IFormatProvider provider, out UserId userId)
    {
        if (!int.TryParse(id, provider, out var idInt))
        {
            userId = default;
            return false;
        }
        
        userId = new UserId(idInt);
        return true;
    }
}