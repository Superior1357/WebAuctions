using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Time.Testing;
using WARSApplication;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using WARSApplication.ErrorResponses.SellErrorResponses;
using Xunit;
using WARSApplication.BusinessMethods;

namespace UnitTests;

public class SellMethodsUnitTests
{
    private readonly TimeProvider _timeProvider = new FakeTimeProvider(
        new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
    );

    private static User TestUser => new()
    {
        Id = new UserId(2)
    };
    
    private static Auction[] MockRepoContent =>
    [
        new TestingAuction()
        {
            Id = new AuctionId(3),
            Seller = new User
            {
                Id = TestUser.Id + 1
            },
            StartTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before1",
            Rules = new EnglishAuctionRules {StartingPrice = 1000},
            Status = AuctionStatus.Pending
        },

        new TestingAuction()
        {
            Id = new AuctionId(6),
            Seller = TestUser,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before2",
            Rules = new DutchAuctionRules { PriceDecrementFrequency = new TimeSpan(0, 1, 0), PriceDecrementRate = 2, StartingPrice = 1000},
            Status = AuctionStatus.Pending
        },

        new TestingAuction()
        {
            Id = new AuctionId(5),
            Seller = TestUser,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before3",
            Rules = new EnglishAuctionRules {StartingPrice = 1000},
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
            new EnglishAuctionRules {StartingPrice = initialPrice}
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
            new EnglishAuctionRules { StartingPrice = initialPrice }
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
                    Assert.Equal(createdAuction.Rules, a.Rules);
                    Assert.Equal(user, a.Seller);
                }
                else // check whether all info stayed the same
                {
                    var correspondingOriginal = addedBeforeContent.First(x => x.Description == a.Description);
                    Assert.Equal(correspondingOriginal.StartTime, a.StartTime);
                    Assert.Equal(correspondingOriginal.EndTime, a.EndTime);
                    Assert.Equal(createdAuction.Rules, a.Rules);
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
            new EnglishAuctionRules { StartingPrice = initialPrice }
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
            new EnglishAuctionRules { StartingPrice = initialPrice }
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

        var existingAuction = new TestingAuction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Rules = new EnglishAuctionRules {StartingPrice = 1000}
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

        var existingAuction = new TestingAuction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Rules = new EnglishAuctionRules {StartingPrice = 1000}
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
                Assert.Equal(existingAuction.Rules, a.Rules);
            }
            else
            {
                var correspondingOriginal = addedBeforeContent.First(x => x.Description == a.Description);
                Assert.Equal(correspondingOriginal.StartTime, a.StartTime);
                Assert.Equal(correspondingOriginal.EndTime, a.EndTime);
                Assert.Equal(correspondingOriginal.Rules, a.Rules);
                Assert.Equal(correspondingOriginal.Seller, a.Seller);
            }
        });
    }
    
    [Fact]
    public async Task Sell_Update_Returns_Error_When_Updated_Auction_Does_Not_Belong_To_User_Async()
    {
        // Arrange
        var user = TestUser;

        var existingAuction = new TestingAuction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Rules = new EnglishAuctionRules {StartingPrice = 1000}
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
        
        var existingAuction = new TestingAuction
        {
            Id = new AuctionId(4),
            Seller = user,
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Rules = new EnglishAuctionRules {StartingPrice = 1000},
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
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Pending, mockRepo)).Value.ToList();
        
        // Assert
        Assert.Equal(2, result.Count);
        foreach (var id in userAuctionIds)
        {
            Assert.Contains(id, result);
        }
    }

    [Fact]
    public async Task Sell_List_Returns_Users_Active_Auctions_Async()
    {
        // Arrange
        var user = TestUser;

        var listedAuction = new TestingAuction
        {
            Id = new AuctionId(7),
            Seller = user,
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Rules = new EnglishAuctionRules {StartingPrice = 1000},
            Status = AuctionStatus.Active
        };

        var addedBeforeContent = MockRepoContent.Append(listedAuction).ToList();

        var mockRepo = new MockAuctionRepository(addedBeforeContent);

        // Act
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Active, mockRepo)).Value.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(listedAuction.Id, result[0]);
    }
    
    [Fact]
    public async Task Sell_List_Returns_Users_Finished_Auctions_Async()
    {
        // Arrange
        var user = TestUser;

        var listedAuction = new TestingAuction
        {
            Id = new AuctionId(7),
            Seller = user,
            StartTime = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            Rules = new EnglishAuctionRules {StartingPrice = 1000},
            Status = AuctionStatus.Finished
        };

        var addedBeforeContent = MockRepoContent.Append(listedAuction).ToList();

        var mockRepo = new MockAuctionRepository(addedBeforeContent);

        // Act
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Active, mockRepo)).Value.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(listedAuction.Id, result[0]);
    }

    [Fact]
    public async Task Sell_Info_Returns_Auctions_Info_Async()
    {
        // Arrange
        var user = TestUser;

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        var userAuctionIds = new List<AuctionId> { addedBeforeContent[1].Id, addedBeforeContent[2].Id };

        // Act
        var response = await SellMethods.GetInfoAsync(user, userAuctionIds, mockRepo);
        
        
        // Assert
        var result = Assert.IsType<Ok<IEnumerable<SellerAuctionInfo>>>(response.Result).Value.ToList();
        
        Assert.Equal(2, result.Count);

        for (int i = 0; i < result.Count; i++)
        {
            var a = result[i].Auction;
            var correspondingOriginal = addedBeforeContent.First(au => au.Id == userAuctionIds[i]);
            
            Assert.Equal(correspondingOriginal.Id, a.Id); 
            Assert.Equal(correspondingOriginal.Description, a.Description);
            Assert.Equal(correspondingOriginal.StartTime, a.StartTime);
            Assert.Equal(correspondingOriginal.EndTime, a.EndTime);
            Assert.Equal(correspondingOriginal.Rules, a.Rules);
            Assert.Equal(correspondingOriginal.Seller, a.Seller);
            Assert.Equal(correspondingOriginal.Status, a.Status);
        }
    }

    [Fact]
    public async Task Sell_Info_Auction_Id_Not_Users_Throws_An_Error_Async()
    {
        // Arrange
        var user = TestUser;

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        var userAuctionIds = new List<AuctionId> { addedBeforeContent[1].Id, addedBeforeContent[0].Id };

        // Act
        var response = await SellMethods.GetInfoAsync(user, userAuctionIds, mockRepo);

        // Assert
        Assert.IsType<NotFound<AuctionIdNotFound>>(response.Result);
    }
}