namespace Frontend.Miscellaneous;

/// <summary>
/// Interface for a Jwt token bearer.
/// Handles the storage and retrieval of the token.
/// </summary>
public interface ITokenBearer
{
    /// <summary>
    ///  Store and retrieve the Jwt token.
    /// </summary>
    public string? Token { get; set; }
}

/// <summary>
/// A concrete TokenBearer implementation.
/// </summary>
public class TokenBearer : ITokenBearer
{
    public string? Token { get; set; }   
}