namespace WARSApplication.ErrorResponses;
/// <summary>
/// Error response for user id not found.
/// </summary>
/// <param name="Message"></param>
public record UserIdNotFound(string Message) : ErrorResponse(Message);