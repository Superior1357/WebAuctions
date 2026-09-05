using WARSApplication.Utils;

namespace UnitTests.HelperClasses;

public record MockCurrentTime(DateTimeOffset Now) : ICurrentTime;