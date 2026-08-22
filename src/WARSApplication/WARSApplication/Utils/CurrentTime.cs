namespace WARSApplication.Utils;

public class CurrentTime(TimeProvider timeProvider) : ICurrentTime
{
    public DateTimeOffset Now { get; }
}