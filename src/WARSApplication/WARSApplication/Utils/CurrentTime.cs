namespace WARSApplication.Utils;

/// <summary>
/// Current time implementation. Uses the time provider to get the current time.
/// The time is fetched only once. The class is supposed to be used as a scoped service.
/// </summary>
/// <param name="timeProvider">a current time provider</param>
public class CurrentTime(TimeProvider timeProvider) : ICurrentTime
{
    public DateTimeOffset Now { get; } = timeProvider.GetUtcNow();
}