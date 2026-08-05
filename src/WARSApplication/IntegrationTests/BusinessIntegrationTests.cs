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
using WARSApplication;

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

public class BusinessEndpointFunctionalityTests : IAsyncLifetime
{
    private TestingWARSAppFactory _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new TestingWARSAppFactory();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Sell_Create_ReturnsOkStatusCodeAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db
        await db.SaveChangesAsync();

        var payload = new CreatedAuction
        (
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            "flower",
            new DutchAuctionRule(1000)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/sell/create", payload);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected OK but got {response.StatusCode}. Response: {responseBody}"
        );
    }

    [Fact]
    public async Task Sell_Create_CreatesAuctionAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db
        await db.SaveChangesAsync();

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
        Assert.Single(db.Auctions);
    }

    [Fact]
    public async Task Sell_Update_ReturnsOkStatusCodeWhenValidRequestIdAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();

        // Act
        var response = await _client.PatchAsJsonAsync("/sell/update",
            new AuctionUpdateRequest(auction.Id, new DateTimeOffset(2025, 1, 1, 1, 1, 1, TimeSpan.Zero)));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Sell_Update_UpdatesDateAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();

        // Act
        var response = await _client.PatchAsJsonAsync("/sell/update",
            new AuctionUpdateRequest(auction.Id, new DateTimeOffset(2025, 1, 1, 1, 1, 1, TimeSpan.Zero)));

        // Assert

        var updatedAuction = await db.Auctions.FindAsync(auction.Id);

        Assert.NotNull(updatedAuction);
        Assert.Equal(2025, updatedAuction.EndTime.Year);
    }

    [Fact]
    public async Task Sell_Update_ReturnsNotFoundWhenIdIsNotUsedAsync()
    {
        // Arrange
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();

        // Act
        var response = await _client.PatchAsJsonAsync("/sell/update",
            new AuctionUpdateRequest(new AuctionId(auction.Id.Id + 10),
                new DateTimeOffset(2025, 1, 1, 1, 1, 1, TimeSpan.Zero)));

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Sell_List_ReturnsOkStatusCodeAsync()
    {
        // Arrange
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/sell/list/Pending");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Sell_List_Pending_Returns_Auctions_EnumerableAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/sell/list/Pending");
        
        // Assert
        var auctions = await response.Content.ReadFromJsonAsync<IEnumerable<AuctionId>>();
        
        Assert.NotNull(auctions);
        Assert.Single(auctions);
    }

    [Fact]
    public async Task Sell_Info_ReturnsOkAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();
        
        
        var query = new List<AuctionId> {new (auction.Id.Id)};

        // Act
        var response = await _client.PostAsJsonAsync("/sell/info", query);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Sell_Info_ReturnsNotFoundWhenIdIsNotUsedAsync()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

        var auction = db.Auctions.Add(
            new Auction
            {
                Seller = TestData.DummyUser,
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Description = "flower",
                Type = AuctionType.Dutch,
                InitialPrice = 1000
            }
        ).Entity;

        await db.SaveChangesAsync();
        var query = new List<AuctionId> {new (auction.Id.Id + 10)};

        // Act
        var response = await _client.PostAsJsonAsync("/sell/info", query);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
    }

// TODO: other tests like this (not concrete, just generic tests); but implement this method first so that I see it works
}