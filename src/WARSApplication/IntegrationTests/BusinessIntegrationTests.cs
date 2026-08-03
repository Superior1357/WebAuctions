using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IntegrationTests;

using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

// TODO: auth tests in a separate file

// TODO: everything should be set up correctly now, just implement the business tests (and add a business user id to db)


public class MinimalDummyAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> o, ILoggerFactory l, UrlEncoder e)
    : AuthenticationHandler<AuthenticationSchemeOptions>(o, l, e)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-123") };
        var identity = new ClaimsIdentity(claims, "Test");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
    }
}

public class BusinessEndpointFunctionalityTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWARSAppFactory _factory;
    private readonly HttpClient _client;

    public BusinessEndpointFunctionalityTests(TestingWARSAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();
    
    [Fact]
    public void Sell_Create_CreatesAuction()
    {
        // Arrange
        var u = 4;

        // Act
        var response = 4;

        // Assert
        Assert.Fail();
    }
}
