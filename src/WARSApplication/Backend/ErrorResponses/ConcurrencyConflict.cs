namespace Backend.ErrorResponses;

public record ConcurrencyConflictResponse(string Message) : ErrorResponse(Message);