using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Time.Testing;
using WARSApplication;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses.SellErrorResponses;

namespace UnitTests;
using Xunit;
using WARSApplication.BusinessMethods;

public class Tests
{
    private readonly TimeProvider _timeProvider = new FakeTimeProvider(
        new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
    );

    private static User TestUser { get; } = new()
    {
        Id = new UserId(2),
        IdentityId = "123456789"
    };

    private static Auction[] MockRepoContent { get; } =
    [
        new Auction
        {
            Id = new AuctionId(3),
            Seller = new User
            {
                Id = TestUser.Id + 1,
                IdentityId = "AnotherUser"
            },
            StartTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before1",
            Type = AuctionType.English,
            InitialPrice = 1000
        },

        new Auction
        {
            Id = new AuctionId(6),
            Seller = TestUser,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before2",
            Type = AuctionType.Dutch,
            InitialPrice = 1000
        },

        new Auction
        {
            Id = new AuctionId(5),
            Seller = TestUser,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before3",
            Type = AuctionType.English,
            InitialPrice = 1000
        }
    ];
    
    [Fact]
    public async Task Sell_Create_Returns_OK_When_Request_Valid_Async()
    {
        // Arrange
        var user = TestUser; 

        var initialPrice = 1000;
        var createdAuction = new CreatedAuction
        (
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "test",
            new EnglishAuctionRule(initialPrice)
        );

        var mockRepo = new MockAuctionRepository(MockRepoContent);
        
        // Act
        var result = await SellMethods.CreateAuctionAsync(user, createdAuction, _timeProvider, mockRepo);
        
        // Assert
        Assert.IsType<Ok>(result.Result);
    }
    
    [Fact]
    public async Task Sell_Create_Adds_Auction_To_Repo_Async()
    {
        // Arrange
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new CreatedAuction
        (
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "created",
            new EnglishAuctionRule(initialPrice)
        );

        var mockRepo = new MockAuctionRepository(MockRepoContent);
        
        // Act
        _ = await SellMethods.CreateAuctionAsync(user, createdAuction, _timeProvider, mockRepo);
        
        // Assert

        var addedBeforeContent = MockRepoContent;
        Assert.Collection(mockRepo.CommitedRepoContent,
            a =>
            {
                if (!a.Description.StartsWith("added_before"))
                {
                    Assert.Equal(createdAuction.Description, a.Description);
                    Assert.Equal(createdAuction.StartTime, a.StartTime);
                    Assert.Equal(createdAuction.EndTime, a.EndTime);
                    Assert.Equal((AuctionType)createdAuction.Rule, a.Type);
                    Assert.Equal(initialPrice, a.InitialPrice);
                    Assert.Equal(user, a.Seller);
                }
                else // check whether all info stayed the same
                {
                    var correspondingOriginal = addedBeforeContent.First(x => x.Description == a.Description);
                    Assert.Equal(correspondingOriginal.StartTime, a.StartTime);
                    Assert.Equal(correspondingOriginal.EndTime, a.EndTime);
                    Assert.Equal(correspondingOriginal.Type, a.Type);
                    Assert.Equal(correspondingOriginal.InitialPrice, a.InitialPrice);
                    Assert.Equal(correspondingOriginal.Seller, a.Seller);
                }
            }
        );
    }
    
    [Fact]
    public async Task Sell_Create_Returns_Error_When_Scheduling_Auction_To_The_Past_Async()
    {
        // Arrange
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new CreatedAuction
        (
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "test",
            new EnglishAuctionRule(initialPrice)
            );

        var mockRepo = new MockAuctionRepository(MockRepoContent);
        
        // Act
        var result = await SellMethods.CreateAuctionAsync(user, createdAuction, _timeProvider, mockRepo);
        
        // Assert
        Assert.IsType<BadRequest<InvalidAuctionParameters>>(result.Result);
    }
    
    [Fact]
    public async Task Sell_Create_Returns_Error_When_Auction_Start_Later_Than_Auction_End_Async()
    {
        // Arrange
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new CreatedAuction
        (
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "test",
            new EnglishAuctionRule(initialPrice)
        );

        var mockRepo = new MockAuctionRepository(MockRepoContent);
        
        // Act
        var result = await SellMethods.CreateAuctionAsync(user, createdAuction, _timeProvider, mockRepo);
        
        // Assert
        Assert.IsType<BadRequest<InvalidAuctionParameters>>(result.Result);
    }
    
    [Fact]
    public async Task Sell_Update_Returns_OK_When_Request_Valid_Async()
    {
        // Arrange
        var user = TestUser;

        var existingAuction = new Auction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Type = AuctionType.English,
            InitialPrice = 1000
        };
        
        var mockRepo = new MockAuctionRepository(MockRepoContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2025,1,1,1,1,1,1, TimeSpan.Zero));
        
        // Act
        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, _timeProvider, mockRepo);
        
        // Assert
        Assert.IsType<Ok>(result.Result);
    }
    
    [Fact]
    public async Task Sell_Update_Updates_Auction_Async()
    {
        // Arrange
        var user = TestUser;

        var existingAuction = new Auction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Type = AuctionType.English,
            InitialPrice = 1000
        };
        
        var mockRepo = new MockAuctionRepository(MockRepoContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2025,1,1,1,1,1,1, TimeSpan.Zero));
        
        // Act
        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, _timeProvider, mockRepo);
        
        // Assert
        var addedBeforeContent = MockRepoContent;
        Assert.True(mockRepo.AllChangesCommited);
        Assert.Collection(mockRepo.CommitedRepoContent, a =>
        {
            if (!a.Description.StartsWith("added_before"))
            {
                Assert.Equal(existingAuction.Description, a.Description);
                Assert.Equal(existingAuction.StartTime, a.StartTime);
                Assert.Equal(updateRequest.NewEnd, a.EndTime);
                Assert.Equal(existingAuction.Type, a.Type);
                Assert.Equal(existingAuction.InitialPrice, a.InitialPrice);
            }
            else
            {
                var correspondingOriginal = addedBeforeContent.First(x => x.Description == a.Description);
                Assert.Equal(correspondingOriginal.StartTime, a.StartTime);
                Assert.Equal(correspondingOriginal.EndTime, a.EndTime);
                Assert.Equal(correspondingOriginal.Type, a.Type);
                Assert.Equal(correspondingOriginal.InitialPrice, a.InitialPrice);
                Assert.Equal(correspondingOriginal.Seller, a.Seller);
            }
        });
    }
    
    [Fact]
    public async Task Sell_Update_Returns_Error_When_Updated_Auction_Does_Not_Belong_To_User_Async()
    {
        // Arrange
        var user = TestUser;

        var existingAuction = new Auction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Type = AuctionType.English,
            InitialPrice = 1000
        };
        
        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent.Append(existingAuction).ToList());
        
        var updateRequest = new AuctionUpdateRequest(addedBeforeContent[0].Id, new DateTimeOffset(2027,1,1,1,1,1,1, TimeSpan.Zero));
        
        // Act
        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, _timeProvider, mockRepo);
        
        // Assert
        Assert.IsType<NotFound<AuctionIdNotFound>>(result.Result);
    }
    
    [Fact]
    public async Task Sell_Update_Returns_Error_When_Trying_To_Shorten_Ongoing_Auction_Async()
    {
        // Arrange
        var user = TestUser;
        
        var existingAuction = new Auction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Type = AuctionType.English,
            InitialPrice = 1000,
            Status = AuctionStatus.Active
        };
        
        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2023,1,1,1,1,1,1, TimeSpan.Zero));
        
        // Act
        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, _timeProvider, mockRepo);
        
        // Assert
        Assert.IsType<BadRequest<InvalidAuctionParameters>>(result.Result);
    }
    
    [Fact]
    public async Task Sell_List_Returns_Users_Pending_Auctions_Async()
    {
        // Arrange
        var user = TestUser;
        
        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        var userAuctionIds = new List<AuctionId> { addedBeforeContent[1].Id, addedBeforeContent[2].Id };
        
        // Act
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Pending, mockRepo)).ToList();
        
        // Assert
        Assert.Equal(2, result.Count);
        foreach (var id in userAuctionIds)
        {
            Assert.Contains(id, result);
        }
    }
    
    // TODO: list tests covering active and finished
    // TODO: complete sell methods
    // TODO: complete buy methods
    // TODO: auth methods? -- last, after everything else is completed
}