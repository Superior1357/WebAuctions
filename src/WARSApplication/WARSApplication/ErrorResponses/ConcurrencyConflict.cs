namespace WARSApplication.ErrorResponses;

public record ConcurrencyConflictResponse(string Message) : ErrorResponse(Message);