namespace Frontend;

public interface ITokenBearer
{
    public string? Token { get; set; }
}

public class TokenBearer : ITokenBearer
{
    public string? Token { get; set; }   
}