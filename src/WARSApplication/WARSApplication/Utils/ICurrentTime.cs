namespace WARSApplication.Utils;

/// <summary>
/// Interface for getting the current time
/// </summary>
public interface ICurrentTime
{
    /// <summary>
    /// The current time
    /// </summary>
    DateTimeOffset Now { get; }
}