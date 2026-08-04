using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using System.Net.Http.Json;

namespace IntegrationTests;

using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

// TODO: auth tests in a separate file

// TODO: everything should be set up correctly now, just implement the business tests (and add a business user id to db)

public static class TestData
{
    public static readonly string AuthUserId = "woenfon";
    public static readonly User DummyUser = new()
    {
        IdentityId = AuthUserId
    };
}

public class MinimalDummyAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> o, ILoggerFactory l, UrlEncoder e)
    : AuthenticationHandler<AuthenticationSchemeOptions>(o, l, e)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, TestData.AuthUserId) };
        var identity = new ClaimsIdentity(claims, "Test");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
    }
}

public class BusinessEndpointFunctionalityTests : IClassFixture<TestingWARSAppFactory>, IAsyncLifetime
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
    public async Task Sell_Create_ReturnsOkStatusCodeAsync()
    {
        // Arrange
        await _factory.SeedAsync(async db => db.Users.Add(TestData.DummyUser)); // Add a dummy user to db
        var payload = new CreatedAuction
        (
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            "flower",
            new DutchAuctionRule(1000)
        );
        
        // Act
        var response = await _client.PostAsJsonAsync("/sell/create", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Sell_Create_CreatesAuctionAsync()
    {
        // Arrange
        await _factory.SeedAsync(async db => db.Users.Add(TestData.DummyUser)); // Add a dummy user to db
        var payload = new CreatedAuction
        (
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            "flower",
            new DutchAuctionRule(1000)
        );
        
        // Act
        var response = await _client.PostAsJsonAsync("/sell/create", payload);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        
        Assert.Single(db.Auctions);
    }
    
    // TODO: other tests like this (not concrete, just generic tests); but implement this method first so that I see it works
}