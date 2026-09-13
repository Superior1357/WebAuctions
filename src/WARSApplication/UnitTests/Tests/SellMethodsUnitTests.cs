using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;
using UnitTests.HelperClasses;
using WARSApplication;
using WARSApplication.BusinessMethods;
using WARSApplication.Data;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using WARSApplication.ErrorResponses.SellErrorResponses;
using WARSApplication.Utils;

namespace UnitTests.Tests;

[TestSubject(typeof(SellMethods))]
public class SellMethodsUnitTests
{
    private readonly ICurrentTime _currentTime = new MockCurrentTime(
        new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
    );

    private static User TestUser => User.Restore(new UserId(2));

    private static User MakeUser(int id) => User.Restore(new UserId(id));

    private static void AssertAuctionMatches(Auction expected, Auction actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.StartTime, actual.StartTime);
        Assert.Equal(expected.EndTime, actual.EndTime);
        Assert.Equal(expected.Seller, actual.Seller);
        
        switch (expected)
        {
            case EnglishAuction englishExpected:
            {
                var englishActual = Assert.IsType<EnglishAuction>(actual);
                Assert.Equal(englishExpected.StartingPrice, englishActual.StartingPrice);
                break;
            }
            case DutchAuction dutchExpected:
            {
                var dutchActual = Assert.IsType<DutchAuction>(actual);
                Assert.Equal(dutchExpected.StartingPrice, dutchActual.StartingPrice);
                Assert.Equal(dutchExpected.PriceDecrementRate, dutchActual.PriceDecrementRate);
                Assert.Equal(dutchExpected.PriceDecrementFrequency, dutchActual.PriceDecrementFrequency);
                break;
            }
            case FirstPriceSealedBidAuction:
                Assert.IsType<FirstPriceSealedBidAuction>(actual);
                break;
        }
    }

    private static Auction[] MockRepoContent =>
    [
        EnglishAuction.Restore(
            new AuctionId(3),
            "added_before1",
            MakeUser(TestUser.Id.Id + 1),
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        ),
        
        DutchAuction.Restore(
            new AuctionId(6),
            "added_before2",
            TestUser,
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            null,
            1000,
            2,
            new TimeSpan(0, 1, 0)
        ),
        
        EnglishAuction.Restore(
            new AuctionId(5),
            "added_before3",
            TestUser,
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        )
    ];

    private static Dictionary<AuctionId, SellerAuctionInfo> AuctionIdToSellerAuctionInfoDictionary => new()
    {
        [new AuctionId(3)] = new EnglishSellerAuctionInfo
        {
            Id = new AuctionId(3),
            Description = "added_before1",
            StartTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            StartingPrice = 1000,
            Info = new PendingStatus()
        },

        [new AuctionId(5)] = new EnglishSellerAuctionInfo
        {
            Id = new AuctionId(5),
            Description = "added_before3",
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            StartingPrice = 1000,
            Info = new PendingStatus()
        },

        [new AuctionId(6)] = new DutchSellerAuctionInfo
        {
            Id = new AuctionId(6),
            Description = "added_before2",
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            StartingPrice = 1000,
            PriceDecrementRate = 2,
            PriceDecrementFrequency = new TimeSpan(0, 1, 0),
            Info = new PendingStatus()
        }
    };

    private static IDataConverter<Auction, SellerAuctionInfo> AuctionToBuyerInfoConverter =>
        new MockAuctionToSellerAuctionInfoConverter(AuctionIdToSellerAuctionInfoDictionary);
    
    [Fact]
    public async Task Sell_Create_Returns_OK_When_Request_Valid_Async()
    {
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new EnglishCreatedAuction {
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            StartingPrice = initialPrice
        };

        var mockRepo = new MockAuctionRepository(MockRepoContent);

        var result = await SellMethods.CreateAuctionAsync(user, createdAuction, _currentTime, mockRepo);

        Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task Sell_Create_Adds_Auction_To_Repo_Async()
    {
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new EnglishCreatedAuction {
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "created",
            StartingPrice = initialPrice
        };

        var mockRepo = new MockAuctionRepository(MockRepoContent);

        _ = await SellMethods.CreateAuctionAsync(user, createdAuction, _currentTime, mockRepo);

        var addedBeforeContent = MockRepoContent;

        Assert.Equal(4, mockRepo.CommitedRepoContent.Count);

        foreach (var a in mockRepo.CommitedRepoContent)
        {
            if (!a.Description.Description.StartsWith("added_before"))
            {
                Assert.Equal(createdAuction.Description, a.Description);
                Assert.Equal(createdAuction.StartTime, a.StartTime);
                Assert.Equal(createdAuction.EndTime, a.EndTime);
                Assert.IsType<EnglishAuction>(a);
                Assert.Equal(user, a.Seller);
            }
            else
            {
                var correspondingOriginal = addedBeforeContent.First(x => x.Description == a.Description);
                AssertAuctionMatches(correspondingOriginal, a);
            }
        }
    }

    [Fact]
    public async Task Sell_Create_Returns_Error_When_Scheduling_Auction_To_The_Past_Async()
    {
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new EnglishCreatedAuction {
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            StartingPrice = initialPrice
        };

        var mockRepo = new MockAuctionRepository(MockRepoContent);

        var result = await SellMethods.CreateAuctionAsync(user, createdAuction, _currentTime, mockRepo);

        Assert.IsType<UnprocessableEntity<InvalidAuctionParameters>>(result.Result);
    }

    [Fact]
    public async Task Sell_Create_Returns_Error_When_Auction_Start_Later_Than_Auction_End_Async()
    {
        var user = TestUser;

        var initialPrice = 1000;
        var createdAuction = new EnglishCreatedAuction {
            StartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "test",
            StartingPrice = initialPrice
        };

        var mockRepo = new MockAuctionRepository(MockRepoContent);

        var result = await SellMethods.CreateAuctionAsync(user, createdAuction, _currentTime, mockRepo);

        Assert.IsType<UnprocessableEntity<InvalidAuctionParameters>>(result.Result);
    }

    [Fact]
    public async Task Sell_Update_Returns_OK_When_Request_Valid_Async()
    {
        var user = TestUser;

        var existingAuction = EnglishAuction.Restore(
            new AuctionId(4),
            "test",
            user,
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );

        var mockEvaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus> { [new AuctionId(4)] = AuctionStatus.Pending },
            new Dictionary<AuctionId, WinnerInfo?> { [new AuctionId(4)] = null },
            new Dictionary<AuctionId, Price?> { [new AuctionId(4)] = 1000 }
        );
        
        var mockRepo = new MockAuctionRepository(MockRepoContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2025, 1, 1, 1, 1, 1, 1, TimeSpan.Zero));

        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, mockEvaluator, mockRepo);

        Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task Sell_Update_Updates_Auction_Async()
    {
        var user = TestUser;

        var existingAuction = EnglishAuction.Restore(
            new AuctionId(4),
            "test",
            user,
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );

        var mockEvaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus> { [new AuctionId(4)] = AuctionStatus.Pending },
            new Dictionary<AuctionId, WinnerInfo?> { [new AuctionId(4)] = null },
            new Dictionary<AuctionId, Price?> { [new AuctionId(4)] = 1000 }
        );
        
        var mockRepo = new MockAuctionRepository(MockRepoContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2025, 1, 1, 1, 1, 1, 1, TimeSpan.Zero));

        _ = await SellMethods.UpdateAuctionAsync(user, updateRequest, mockEvaluator, mockRepo);

        var addedBeforeContent = MockRepoContent;
        Assert.True(mockRepo.AllChangesCommited);
        Assert.Equal(4, mockRepo.CommitedRepoContent.Count);        
        
        foreach (var a in mockRepo.CommitedRepoContent) { 
            if (!a.Description.Description.StartsWith("added_before"))
            {
                Assert.Equal(existingAuction.Description, a.Description);
                Assert.Equal(existingAuction.StartTime, a.StartTime);
                Assert.Equal(updateRequest.NewEnd, a.EndTime);
                Assert.IsType<EnglishAuction>(a);
            }
            else
            {
                var correspondingOriginal = addedBeforeContent.First(x => x.Description == a.Description);
                AssertAuctionMatches(correspondingOriginal, a);
            }
        }
    }
    
    
    [Fact]
    public async Task Sell_Update_Returns_Error_When_Updated_Auction_Does_Not_Belong_To_User_Async()
    {
        var user = TestUser;

        var existingAuction = EnglishAuction.Restore(
            new AuctionId(4),
            "test",
            MakeUser(TestUser.Id.Id + 1),
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );

        var mockEvaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus> { [new AuctionId(4)] = AuctionStatus.Pending },
            new Dictionary<AuctionId, WinnerInfo?> { [new AuctionId(4)] = null },
            new Dictionary<AuctionId, Price?> { [new AuctionId(4)] = 1000 }
        );
        
        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent.Append(existingAuction).ToList());

        var updateRequest = new AuctionUpdateRequest(addedBeforeContent[0].Id, new DateTimeOffset(2027, 1, 1, 1, 1, 1, 1, TimeSpan.Zero));

        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, mockEvaluator, mockRepo);

        Assert.IsType<NotFound<AuctionIdNotFound>>(result.Result);
    }

    [Fact]
    public async Task Sell_Update_Returns_Error_When_Trying_To_Shorten_Ongoing_Auction_Async()
    {
        var user = TestUser;

        var existingAuction = EnglishAuction.Restore(
            new AuctionId(4),
            "test",
            user,
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );
        
        var mockEvaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus> { [new AuctionId(4)] = AuctionStatus.Pending },
            new Dictionary<AuctionId, WinnerInfo?> { [new AuctionId(4)] = null },
            new Dictionary<AuctionId, Price?> { [new AuctionId(4)] = 1000 }
        );

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2023, 1, 1, 1, 1, 1, 1, TimeSpan.Zero));

        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, mockEvaluator, mockRepo);

        Assert.IsType<UnprocessableEntity<InvalidAuctionParameters>>(result.Result);
    }
    
    [Fact]
    public async Task Sell_Update_Returns_Error_When_Trying_To_Update_Finished_Auction_Async()
    {
        var user = TestUser;

        var existingAuction = EnglishAuction.Restore(
            new AuctionId(4),
            "test",
            user,
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );

        var mockEvaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus> { [new AuctionId(4)] = AuctionStatus.Finished },
            new Dictionary<AuctionId, WinnerInfo?> { [new AuctionId(4)] = null },
            new Dictionary<AuctionId, Price?> { [new AuctionId(4)] = 1000 }
        );

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent.Append(existingAuction).ToList());
        var updateRequest = new AuctionUpdateRequest(existingAuction.Id, new DateTimeOffset(2026, 1, 1, 1, 1, 1, 1, TimeSpan.Zero));

        var result = await SellMethods.UpdateAuctionAsync(user, updateRequest, mockEvaluator, mockRepo);

        Assert.IsType<UnprocessableEntity<InvalidAuctionParameters>>(result.Result);
    }

    [Fact]
    public async Task Sell_List_Returns_Users_Pending_Auctions_Async()
    {
        // Arrange
        var user = TestUser;

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        var userAuctionIds = new List<AuctionId> { addedBeforeContent[1].Id, addedBeforeContent[2].Id };

        var evaluator = new MockAuctionEvaluator(
                new Dictionary<AuctionId, AuctionStatus>
                {
                    { addedBeforeContent[0].Id, AuctionStatus.Pending },
                    { addedBeforeContent[1].Id, AuctionStatus.Pending },
                    { addedBeforeContent[2].Id, AuctionStatus.Pending }
                },
                new Dictionary<AuctionId, WinnerInfo?>
                {
                    { addedBeforeContent[0].Id, null },
                    { addedBeforeContent[1].Id, null },
                    { addedBeforeContent[2].Id, null }
                },
                
                new Dictionary<AuctionId, Price?>
                {
                    { addedBeforeContent[0].Id, 1000 },
                    { addedBeforeContent[1].Id, 1000 },
                    { addedBeforeContent[2].Id, 1000 }
                }
            );
        // Act
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Pending, evaluator, mockRepo)).Value.ToList();
        
        // Arrange
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

        var listedAuction = EnglishAuction.Restore(
            new AuctionId(7),
            "test",
            user,
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );

        var addedBeforeContent = MockRepoContent.Append(listedAuction).ToList();
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        
        var evaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus>
            {
                { addedBeforeContent[0].Id, AuctionStatus.Pending },
                { addedBeforeContent[1].Id, AuctionStatus.Pending },
                { addedBeforeContent[2].Id, AuctionStatus.Pending },
                { listedAuction.Id, AuctionStatus.Active }
            },
            new Dictionary<AuctionId, WinnerInfo?>
            {
                { addedBeforeContent[0].Id, null },
                { addedBeforeContent[1].Id, null },
                { addedBeforeContent[2].Id, null },
                { listedAuction.Id, null }
            },
            
            new Dictionary<AuctionId, Price?>
            {
                { addedBeforeContent[0].Id, 1000 },
                { addedBeforeContent[1].Id, 1000 },
                { addedBeforeContent[2].Id, 1000 },
                { listedAuction.Id, 1000 }
            }
        );
        
        // Act
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Active, evaluator, mockRepo)).Value.ToList();
        
        // Assert
        Assert.Single(result);
        Assert.Equal(listedAuction.Id, result[0]);
    }

    [Fact]
    public async Task Sell_List_Returns_Users_Finished_Auctions_Async()
    {
        // Arrange
        var user = TestUser;

        var listedAuction = EnglishAuction.Restore(
            new AuctionId(7),
            "test",
            user,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );

        var addedBeforeContent = MockRepoContent.Append(listedAuction).ToList();
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        
        var evaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus>
            {
                { addedBeforeContent[0].Id, AuctionStatus.Pending },
                { addedBeforeContent[1].Id, AuctionStatus.Pending },
                { addedBeforeContent[2].Id, AuctionStatus.Pending },
                { listedAuction.Id, AuctionStatus.Finished }
            },
            
            new Dictionary<AuctionId, WinnerInfo?>
            {
                { addedBeforeContent[0].Id, null },
                { addedBeforeContent[1].Id, null },
                { addedBeforeContent[2].Id, null },
                { listedAuction.Id, null }
            },
            
            new Dictionary<AuctionId, Price?>
            {
                { addedBeforeContent[0].Id, 1000 },
                { addedBeforeContent[1].Id, 1000 },
                { addedBeforeContent[2].Id, 1000 },
                { listedAuction.Id, null }
            }
        );
        
        // Act
        var result = (await SellMethods.ListAsync(user, AuctionStatus.Finished, evaluator, mockRepo)).Value.ToList();
        
        // Assert
        Assert.Single(result);
        Assert.Equal(listedAuction.Id, result[0]);
    }

    [Fact]
    public async Task Sell_Info_Returns_Auctions_Info_Async()
    {
        var user = TestUser;

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        var userAuctionIds = new List<AuctionId> { addedBeforeContent[1].Id, addedBeforeContent[2].Id };

        var response = await SellMethods.GetInfoAsync(user, userAuctionIds, mockRepo, AuctionToBuyerInfoConverter);

        var result = Assert.IsType<Ok<IEnumerable<SellerAuctionInfo>>>(response.Result).Value.ToList();

        Assert.Equal(2, result.Count);

        for (int i = 0; i < result.Count; i++)
        {
            var resultInfo = result[i];
            var correspondingOriginal = AuctionIdToSellerAuctionInfoDictionary[userAuctionIds[i]];
            
            Assert.Equal(correspondingOriginal, resultInfo);
        }
    }

    [Fact]
    public async Task Sell_Info_Auction_Id_Not_Users_Throws_An_Error_Async()
    {
        var user = TestUser;

        var addedBeforeContent = MockRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
        var userAuctionIds = new List<AuctionId> { addedBeforeContent[1].Id, addedBeforeContent[0].Id };

        var response = await SellMethods.GetInfoAsync(user, userAuctionIds, mockRepo, AuctionToBuyerInfoConverter);

        Assert.IsType<NotFound<AuctionIdNotFound>>(response.Result);
    }
}
