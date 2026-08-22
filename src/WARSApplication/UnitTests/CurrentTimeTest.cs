using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using WARSApplication.Utils;

namespace UnitTests;

[TestSubject(typeof(CurrentTime))]
public class CurrentTimeTest
{
    [Fact]
    public void CurrentTime_Now_Returns_Current_Time()
    {
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero));
        
        var currentTime = new CurrentTime(fakeTimeProvider);
        Assert.Equal(fakeTimeProvider.GetUtcNow(), currentTime.Now);
    }
    
    [Fact]
    public void CurrentTime_Now_Caches_Time_Upon_Construction()
    {
        var oldTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        var fakeTimeProvider = new FakeTimeProvider(oldTime);
        var currentTime = new CurrentTime(fakeTimeProvider);
        
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
        
        Assert.Equal(oldTime, currentTime.Now);
    }
}