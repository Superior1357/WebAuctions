using WARSApplication.Utils;

namespace UnitTests;

public record MockCurrentTime(DateTimeOffset Now) : ICurrentTime;