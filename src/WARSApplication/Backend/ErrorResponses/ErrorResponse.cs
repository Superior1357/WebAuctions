namespace Backend.ErrorResponses;

/// <summary>
/// Base class for all API error responses.
/// </summary>
/// <param name="Message"></param>
public abstract record ErrorResponse(string Message);