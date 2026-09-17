using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Backend.Data.APIData.Incoming;
using Backend.Data.DataModel.Business;
using Backend.Data.SharedDataProperties;
using System.Net.Http.Json;
using Backend.Data.APIData.Outgoing;
using Backend.Data.DataModel;

namespace IntegrationTests;

using Xunit;


public static class TestData
{
    public static readonly string AuthUserId = "woenfon";
    public static readonly string AuthUserId2 = "woenfon2";

    internal static DataModelUser DummyUser { get; private set; } = new()
    {
        IdentityId = AuthUserId
    };

    internal static DataModelUser DummyUser2 { get; private set; } = new()
    {
        IdentityId = AuthUserId2
    };

public static void RefreshUser()
    {
        DummyUser = new()
        {
            IdentityId = AuthUserId
        };
        
        DummyUser2 = new()
        {
            IdentityId = AuthUserId2
        };
    }
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
        TestData.RefreshUser();
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
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db
            await db.SaveChangesAsync();
        }
        
        CreatedAuction payload = new DutchCreatedAuction
        {
            StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Description = "flower",
            PriceDecrementFrequency = new TimeSpan(0, 0, 1),
            PriceDecrementRate = 2,
            StartingPrice = 1000
        };

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
        using (var scope = _factory.Services.CreateScope()) 
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db
            await db.SaveChangesAsync();
        }

        CreatedAuction payload = new DutchCreatedAuction
        {
            StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Description = "flower",
            PriceDecrementFrequency = new TimeSpan(0, 0, 1),
            PriceDecrementRate = 2,
            StartingPrice = 1000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/sell/create", payload);

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            Assert.Single(db.Auctions);
        }
    }

    [Fact]
    public async Task Sell_Update_ReturnsOkStatusCodeWhenValidRequestIdAsync()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.PatchAsJsonAsync("/sell/update",
            new AuctionUpdateRequest(auction.Id, new DateTimeOffset(2025, 1, 1, 1, 1, 1, TimeSpan.Zero)));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Sell_Update_UpdatesDateAsync()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.PatchAsJsonAsync("/sell/update",
            new AuctionUpdateRequest(auction.Id, new DateTimeOffset(2025, 1, 1, 1, 1, 1, TimeSpan.Zero)));

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            var updatedAuction = await db.Auctions.FindAsync(auction.Id);

            Assert.NotNull(updatedAuction);
            Assert.Equal(2025, updatedAuction.EndTime.Year);
        }
    }

    [Fact]
    public async Task Sell_Update_ReturnsNotFoundWhenIdIsNotUsedAsync()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser2,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.PatchAsJsonAsync("/sell/update",
            new AuctionUpdateRequest(new AuctionId(auction.Id.Id + 10),
                new DateTimeOffset(2025, 1, 1, 1, 1, 1, TimeSpan.Zero)));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Sell_List_ReturnsOkStatusCodeAsync()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            var auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser2,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/sell/list/Pending");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sell_List_Pending_Returns_Auctions_EnumerableAsync()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            var auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

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
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        var query = new List<AuctionId> {new (auction.Id.Id)};

        // Act
        var response = await _client.PostAsJsonAsync("/sell/info", query);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sell_Info_ReturnsNotFoundWhenIdIsNotUsedAsync()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser2,
                    StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        var query = new List<AuctionId> {auction.Id + 10};

        // Act
        var response = await _client.PostAsJsonAsync("/sell/info", query);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_Bid_ReturnsOkStatusCodeAsync()
    {   
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser2,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000,
                    Bids = [
                        new DataModelBid
                        {
                            Bidder = TestData.DummyUser,
                            Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero),
                            Value = 1000,
                        }
                    ]
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        var bid = new BidRequest(auction.Id, 10_000);
        
        // Act
        var response = await _client.PostAsJsonAsync("/buy/bid", bid);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_Bid_Adds_Bid_Async()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser2,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000,
                    Bids = [
                        new DataModelBid
                        {
                            Bidder = TestData.DummyUser,
                            Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero),
                            Value = 1000,
                        }
                    ]
                }
            ).Entity;

            await db.SaveChangesAsync();
        }
        
        // Act
        
        BidRequest bid;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            var auction = db.Auctions.First();

            bid = new BidRequest(auction.Id, 10_000);
        }
        var response = await _client.PostAsJsonAsync("/buy/bid", bid);
        
        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            Assert.Contains(db.Bids, b => b.Value == 10_000);
        }
    }
    
    [Fact]
    public async Task Buy_Nonexistent_Auction_Bid_ReturnsNotFoundStatusCodeAsync()
    {   
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            await db.Users.AddAsync(TestData.DummyUser); // Add a dummy user to db

            auction = db.Auctions.Add(
                new DutchDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    PriceDecrementFrequency = new TimeSpan(0, 0, 1),
                    PriceDecrementRate = 2,
                    StartingPrice = 1000,
                }
            ).Entity;


            DataModelBid previousBid = new DataModelBid
            {
                Bidder = TestData.DummyUser,
                ModelAuction = auction,
                Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero),
                Value = 1000,
            };

            await db.Bids.AddAsync(previousBid);
            await db.SaveChangesAsync();
        }

        var bid = new BidRequest(auction.Id + 10, 10_000);
        
        // Act
        var response = await _client.PostAsJsonAsync("/buy/bid", bid);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Buy_List_Sellers_Returns_OK_Code_Async()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            var sellerUser = new DataModelUser
            {
                IdentityId = "yes"
            };

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            await db.Users.AddRangeAsync(sellerUser, noSellerUser, TestData.DummyUser);

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/buy/list/sellers");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_List_Sellers_Returns_Correct_Sellers_Async()
    {
        // Arrange
        DataModelUser sellerUser;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddRangeAsync(noSellerUser, TestData.DummyUser);

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/buy/list/sellers");
        
        // Assert
        var content = (await response.Content.ReadFromJsonAsync<IEnumerable<UserId>>()).ToList();
        
        Assert.Equal(2, content.Count);
        Assert.Contains(sellerUser.Id, content);
        Assert.Contains(TestData.DummyUser.Id, content);
    }
    
    [Fact]
    public async Task Buy_List_Not_Finished_Returns_OK_Code_Async()
    {
        // Arrange
        DataModelUser sellerUser;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            sellerUser = new DataModelUser
                {
                    IdentityId = "yes"
                };

            await db.Users.AddRangeAsync(noSellerUser);

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.PostAsJsonAsync("/buy/list/not_finished", sellerUser.Id);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_List_Not_Finished_Returns_Not_Finished_Auctions_Async()
    {
        // Arrange
        DataModelUser sellerUser;
        DataModelAuction sellerAuction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddRangeAsync(noSellerUser, TestData.DummyUser);

            sellerAuction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.PostAsJsonAsync("/buy/list/not_finished", sellerUser.Id);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"Expected success but got {(int)response.StatusCode}: {responseBody}");
        
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<AuctionId>>();
        Assert.Single(content);
        Assert.Equal(sellerAuction.Id, content.First());
        
    }
    
    [Fact]
    public async Task Buy_List_Active_Returns_NotFound_When_User_Not_Found_Async()
    {
        // Arrange
        DataModelUser sellerUser;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddRangeAsync(noSellerUser, TestData.DummyUser);

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/buy/list/active/{sellerUser.Id + 10}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_Won_Returns_OK_Code_Async()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddAsync(TestData.DummyUser);

            var auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            DataModelBid winningBid = new()
            {
                Bidder = TestData.DummyUser,
                ModelAuction = auction,
                Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero),
                Value = 2000,
            };

            await db.Bids.AddAsync(winningBid);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/buy/list/won");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_Won_Returns_Won_Auction_Ids_Async()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddAsync(TestData.DummyUser);

            auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            DataModelBid winningBid = new()
            {
                Bidder = TestData.DummyUser,
                ModelAuction = auction,
                Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero),
                Value = 2000,
            };

            await db.Bids.AddAsync(winningBid);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/buy/list/won");
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<AuctionId>>();
        
        // Assert
        Assert.Single(content);
        Assert.Equal(auction.Id, content.First());
    }
    
    [Fact]
    public async Task Buy_Participated_Returns_OK_Code_Async()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddAsync(TestData.DummyUser);

            var auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            DataModelBid winningBid = new()
            {
                Bidder = TestData.DummyUser,
                ModelAuction = auction,
                Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero),
                Value = 2000,
            };

            await db.Bids.AddAsync(winningBid);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/buy/list/participated");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_Participated_Returns_Won_Auction_Ids_Async()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddAsync(TestData.DummyUser);

            auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            DataModelBid winningBid = new()
            {
                Bidder = TestData.DummyUser,
                ModelAuction = auction,
                Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero),
                Value = 2000,
            };

            await db.Bids.AddAsync(winningBid);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/buy/list/participated");
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<AuctionId>>();
        
        // Assert
        Assert.Single(content);
        Assert.Equal(auction.Id, content.First());
    }
    
    [Fact]
    public async Task Buy_Info_Returns_OK_Status_Code_Async()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddRangeAsync(noSellerUser, TestData.DummyUser);

            auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        AuctionId[] requestedInfo = [auction.Id];
        
        // Act
        var response = await _client.PostAsJsonAsync("/buy/info", requestedInfo);
        
        // Assert
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<BuyerAuctionInfo>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Buy_Info_Returns_Info_Async()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddRangeAsync(noSellerUser, TestData.DummyUser);

            auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        AuctionId[] requestedInfo = [auction.Id];
        
        // Act
        var response = await _client.PostAsJsonAsync("/buy/info", requestedInfo);
        
        // Assert
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<BuyerAuctionInfo>>();
        
        Assert.Single(content);
    }
    
    [Fact]
    public async Task Buy_Info_Returns_NotFound_Auction_Nonexistent_Async()
    {
        // Arrange
        DataModelAuction auction;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

            var noSellerUser = new DataModelUser
            {
                IdentityId = "no"
            };

            var sellerUser = db.Users.Add(
                new DataModelUser
                {
                    IdentityId = "yes"
                }
            ).Entity;

            await db.Users.AddRangeAsync(noSellerUser, TestData.DummyUser);

            auction = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = sellerUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            _ = db.Auctions.Add(
                new EnglishDataModelAuction
                {
                    Seller = TestData.DummyUser,
                    StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Description = "flower",
                    StartingPrice = 1000
                }
            ).Entity;

            await db.SaveChangesAsync();
        }

        AuctionId[] requestedInfo = [auction.Id + 10];
        
        // Act
        var response = await _client.PostAsJsonAsync("/buy/info", requestedInfo);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}