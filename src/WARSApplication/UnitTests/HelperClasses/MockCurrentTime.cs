using Backend.Utils;

namespace UnitTests.HelperClasses;

public record MockCurrentTime(DateTimeOffset Now) : ICurrentTime;