namespace WARSApplication.APIData;

public record struct User(int Id)
{
    public static bool TryParse(string s, IFormatProvider p, out User id)
    {
        var res = int.TryParse(s, p, out var i);  
        id = new User(i);
        return res;
    }
}