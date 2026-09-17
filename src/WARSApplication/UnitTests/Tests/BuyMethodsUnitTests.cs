using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;
using UnitTests.HelperClasses;
using Backend;
using Backend.AuctionEvaluation;
using Backend.Data;
using Backend.Data.APIData.Incoming;
using Backend.Data.APIData.Outgoing;
using Backend.Data.DataModel.Business;
using Backend.Data.SharedDataProperties;
using Backend.ErrorResponses;
using Backend.ErrorResponses.BuyErrorResponses;
using Backend.Utils;
using BuyMethods = Backend.BusinessMethods.BuyMethods;

namespace UnitTests.Tests;
[TestSubject(typeof(BuyMethods))]
public class BuyMethodsUnitTests
{
    private readonly ICurrentTime _currentTime = new MockCurrentTime(
        new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero));
    
    private static User TestUser { get; } = User.Restore(new UserId(2));
    
    private static User MakeUser(int id) => User.Restore(new UserId(id));

    private static Bid MakeBid(int id, decimal value, User bidder, DateTimeOffset time) =>
        Bid.Restore(id, bidder, value, time);
    
    private static Auction[] MockAuctionsRepoContent
    {
        get
        {
            var auction0 = EnglishAuction.Restore(
                new AuctionId(0),
                "added_before1",
                MakeUser(TestUser.Id.Id + 1),
                new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new TestAddOnlyCollection<Bid>([]),
                1000
            );

            var auction1 = EnglishAuction.Restore(
                new AuctionId(1),
                "added_before3",
                TestUser,
                new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new TestAddOnlyCollection<Bid>([]),
                1000
            );

            var auction2 = DutchAuction.Restore(
                new AuctionId(2),
                "added_before2",
                TestUser,
                new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                null,
                1000,
                2,
                new TimeSpan(0, 0, 1)
            );

            var auction3 = EnglishAuction.Restore(
                new AuctionId(3),
                "added_before_active_1",
                MakeUser(TestUser.Id.Id + 1),
                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new TestAddOnlyCollection<Bid>(
                    [
                        MakeBid(0, 1000, TestUser, new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)), 
                        MakeBid(1, 1500, MakeUser(TestUser.Id.Id + 2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)),
                        MakeBid(2, 2000, TestUser, new DateTimeOffset(2021, 3, 1, 1, 1, 2, TimeSpan.Zero))
                    ]
                    ),
                1000
            );

            var auction4 = FirstPriceSealedBidAuction.Restore(
                new AuctionId(4),
                "added_before_active_2",
                MakeUser(TestUser.Id.Id + 2),
                new TestAddOnlyCollection<Bid>([MakeBid(3, 1000, TestUser, new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero))]),
                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2021, 5, 1, 0, 0, 0, TimeSpan.Zero)
            );

            var auction5 = FirstPriceSealedBidAuction.Restore(
                new AuctionId(5),
                "added_before_active_3",
                MakeUser(TestUser.Id.Id + 2),
                new TestAddOnlyCollection<Bid>([]),
                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            );

            return [auction0, auction1, auction2, auction3, auction4, auction5];
        }
    }

    private static IAuctionEvaluator MockAuctionEvaluator => new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus>
            {
                [new AuctionId(0)] = AuctionStatus.Pending,
                [new AuctionId(1)] = AuctionStatus.Pending,
                [new AuctionId(2)] = AuctionStatus.Pending,
                [new AuctionId(3)] = AuctionStatus.Active,
                [new AuctionId(4)] = AuctionStatus.Finished,
                [new AuctionId(5)] = AuctionStatus.Active
            },
            
            new Dictionary<AuctionId, WinnerInfo?>
            {
                [new AuctionId(0)] = null,
                [new AuctionId(1)] = null,
                [new AuctionId(2)] = null,
                [new AuctionId(3)] = null,
                [new AuctionId(4)] = new WinnerInfo(1000, TestUser.Id),
                [new AuctionId(5)] = null
            },
            
            new Dictionary<AuctionId, Price?>
            {
                [new AuctionId(0)] = 1000,
                [new AuctionId(1)] = 1000,
                [new AuctionId(2)] = 1000,
                [new AuctionId(3)] = 2001,
                [new AuctionId(4)] = null,
                [new AuctionId(5)] = null
            }
        );

    [Fact]
    public async Task List_Sellers_Lists_All_Users_Who_Created_Auctions_Async()
    {
        var addedBeforeContent = MockAuctionsRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);

        List<UserId> sellerIds = [addedBeforeContent[0].Seller.Id, addedBeforeContent[1].Seller.Id];

        var result = (await BuyMethods.ListAllSellersAsync(mockRepo)).Value.ToList();

        Assert.Equal(3, result.Count);
        foreach (var id in sellerIds)
        {
            Assert.Contains(id, result);
        }
    }

    [Fact]
    public async Task List_Participated_Lists_All_Auctions_That_User_Participated_In_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var result = (await BuyMethods.ListParticipatedAuctionsAsync(TestUser, mockAuctionRepo)).Value.ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(addedBeforeAuctionContent[3].Id, result);
        Assert.Contains(addedBeforeAuctionContent[4].Id, result);
    }

    [Fact]
    public async Task List_Won_Lists_All_Auctions_That_User_Won_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var result = (await BuyMethods.ListWonAuctionsAsync(TestUser, mockAuctionRepo, MockAuctionEvaluator)).Value.ToList();

        Assert.Single(result);
        Assert.Contains(addedBeforeAuctionContent[4].Id, result);
    }

    [Fact]
    public async Task List_Non_Finished_Auctions_Lists_All_Non_Finished_Auctions_Of_A_Seller_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var response = await BuyMethods.ListNotFinishedAuctionsOfASellerAsync(addedBeforeAuctionContent[4].Seller.Id, mockAuctionRepo, MockAuctionEvaluator);

        var result = Assert.IsType<Ok<IEnumerable<AuctionId>>>(response.Result).Value.ToList();

        Assert.Single(result);
        Assert.Contains(addedBeforeAuctionContent[5].Id, result);
    }

    [Fact]
    public async Task List_Non_Finished_Auctions_Returns_Error_If_User_ID_Does_Not_Exist_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var response = await BuyMethods.ListNotFinishedAuctionsOfASellerAsync(addedBeforeAuctionContent[4].Seller.Id + 1000, mockAuctionRepo, MockAuctionEvaluator);

        Assert.IsType<NotFound<UserIdNotFound>>(response.Result);
    }
    
    [Fact]
    public async Task Bid_Bids_To_An_First_Price_Sealed_Bid_Auction_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var bidRequest = new BidRequest(addedBeforeAuctionContent[5].Id, 10_000);

        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _currentTime, mockAuctionRepo, MockAuctionEvaluator);
        Assert.IsType<Ok>(response.Result);
        Assert.True(mockAuctionRepo.AllChangesCommited);
        Assert.Contains(addedBeforeAuctionContent[5].Id, mockAuctionRepo.UpdatesCalled.Select(u => u.Id));
        
        var repoContent = mockAuctionRepo.CommitedRepoContent;
        var correspondingAuction = repoContent.First(a => a.Id == addedBeforeAuctionContent[5].Id);
        
        var firstPriceCorrespondingAuction = Assert.IsType<FirstPriceSealedBidAuction>(correspondingAuction);
        Assert.Contains(firstPriceCorrespondingAuction.Bids,
            bid => bid.Bidder.Id == TestUser.Id && bid.Value == bidRequest.Amount);
    }
    
    [Fact]
    public async Task Bid_Bids_To_An_Active_English_Auction_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var bidRequest = new BidRequest(addedBeforeAuctionContent[3].Id, 10_000);

        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _currentTime, mockAuctionRepo, MockAuctionEvaluator);
        Assert.IsType<Ok>(response.Result);
        Assert.True(mockAuctionRepo.AllChangesCommited);
        Assert.Contains(addedBeforeAuctionContent[3].Id, mockAuctionRepo.UpdatesCalled.Select(u => u.Id));

        var repoContent = mockAuctionRepo.CommitedRepoContent;
        var correspondingAuction = repoContent.First(a => a.Id == addedBeforeAuctionContent[3].Id);

        var englishCorrespondingAuction = Assert.IsType<EnglishAuction>(correspondingAuction);
        Assert.Contains(englishCorrespondingAuction.Bids,
            bid => bid.Bidder.Id == TestUser.Id && bid.Value == bidRequest.Amount);
    }
    
    [Fact]
    public async Task Bid_Bids_To_An_Active_Dutch_Auction_Async()
    {
        var activeDutchAuction = DutchAuction.Restore(
            new AuctionId(6),
            "added_before_active_dutch",
            MakeUser(TestUser.Id.Id + 2),
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            null,
            1000,
            2,
            new TimeSpan(0, 0, 1)
        );

        var auctionEvaluator = new MockAuctionEvaluator(
            new Dictionary<AuctionId, AuctionStatus> { [new AuctionId(6)] = AuctionStatus.Active },
            new Dictionary<AuctionId, WinnerInfo?> { [new AuctionId(6)] = null },
            new Dictionary<AuctionId, Price?> { [new AuctionId(6)] = 100 }
        );
        var mockAuctionRepo = new MockAuctionRepository([activeDutchAuction]);

        var bidRequest = new BidRequest(activeDutchAuction.Id, 10_000);

        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _currentTime, mockAuctionRepo, auctionEvaluator);
        Assert.IsType<Ok>(response.Result);
        Assert.True(mockAuctionRepo.AllChangesCommited);
        Assert.Contains(activeDutchAuction.Id, mockAuctionRepo.UpdatesCalled.Select(u => u.Id));

        var repoContent = mockAuctionRepo.CommitedRepoContent;
        var correspondingAuction = repoContent.First(a => a.Id == activeDutchAuction.Id);

        var dutchCorrespondingAuction = Assert.IsType<DutchAuction>(correspondingAuction);
        Assert.NotNull(dutchCorrespondingAuction.Bid);
        Assert.Equal(TestUser.Id, dutchCorrespondingAuction.Bid.Bidder.Id);
        Assert.Equal(bidRequest.Amount, dutchCorrespondingAuction.Bid.Value);
    }
    
    [Fact]
    public async Task Bid_Concurrency_Conflict_Solved_When_Commit_Finished_With_A_Concurrency_Exception_Async()
    {
        // Arrange
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent, true);
        var bid = new BidRequest(addedBeforeAuctionContent[5].Id, 10_000);
        
        // Act
        var result = await BuyMethods.BidAsync(TestUser, bid, _currentTime, mockAuctionRepo, MockAuctionEvaluator);
        
        // Assert
        Assert.IsType<Conflict<ConcurrencyConflictResponse>>(result.Result);
    }
    
    [Fact]
    public async Task Bid_Returns_Not_Found_When_Auction_Id_Not_Found_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var bidRequest = new BidRequest(addedBeforeAuctionContent[5].Id + 10000, 10_000);

        var response =
            await BuyMethods.BidAsync(TestUser, bidRequest, _currentTime, mockAuctionRepo, MockAuctionEvaluator);

        Assert.IsType<NotFound<AuctionIdNotFound>>(response.Result);
    }
    
    public static IEnumerable<object[]> InactiveAuctions() => MockAuctionsRepoContent.Where(
            a => MockAuctionEvaluator.EvaluateStatus(a) != AuctionStatus.Active
        ).Select(a => new object[] {a});  
    
    [Theory]
    [MemberData(nameof(InactiveAuctions))]
    internal async Task Bid_Returns_Error_When_Auction_Inactive_Async(Auction repoAuction)
    {
        var mockAuctionRepo = new MockAuctionRepository([repoAuction]);

        var bidRequest = new BidRequest(repoAuction.Id, 10_000);
        
        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _currentTime, mockAuctionRepo, MockAuctionEvaluator);
        Assert.IsType<UnprocessableEntity<BidAuctionRulesViolationResponse>>(response.Result);
    }

    public static IEnumerable<object[]> ActiveAuctions() => new List<object[]>
    {
        new object[]
        {
            EnglishAuction.Restore(
                new AuctionId(3),
                "added_before_active_1",
                MakeUser(TestUser.Id.Id + 1),
                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new TestAddOnlyCollection<Bid>(
                    [
                    ]
                ),
                10
            )
        },
        
        new object[]
        {
            DutchAuction.Restore(
            new AuctionId(3),
            "added_before2",
            MakeUser(TestUser.Id.Id + 1),
            new DateTimeOffset(2021, 12, 30, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            null,
            10,
            2,
            new TimeSpan(1,0, 0, 0)
            )
        } 
    };
    
    [Theory]
    [MemberData(nameof(ActiveAuctions))]
    internal async Task Bid_Returns_Error_When_Bid_Price_Too_Low_Dutch_English_Auctions_Async(Auction activeAuction)
    {
        var mockAuctionRepo = new MockAuctionRepository([activeAuction]);
        
        var bidRequest = new BidRequest(activeAuction.Id, 100);
        
        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _currentTime, mockAuctionRepo, MockAuctionEvaluator);
        Assert.IsType<UnprocessableEntity<BidAuctionRulesViolationResponse>>(response.Result);
    }
    
    [Fact]
    public async Task Bid_Returns_Error_When_Bidding_Twice_To_First_Price_Sealed_Bid_Async()
    {
        var repoContent = FirstPriceSealedBidAuction.Restore(
            new AuctionId(5),
            "added_before_active_3",
            MakeUser(TestUser.Id.Id + 2),
            new TestAddOnlyCollection<Bid>([
                MakeBid(100, 10, TestUser, new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero))
            ]),
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        
        var bidRequest = new BidRequest(repoContent.Id, 10_000);
        
        var mockRepo = new MockAuctionRepository([repoContent]);
        var response = await BuyMethods.BidAsync(
                TestUser, bidRequest, _currentTime, mockRepo, MockAuctionEvaluator
            );

        Assert.IsType<UnprocessableEntity<BidAuctionRulesViolationResponse>>(response.Result);
    }

    private class MockAuctionToBuyerAuctionInfoConverter(Dictionary<AuctionId, BuyerAuctionInfo> mapping) : IDataConverter<Auction, BuyerAuctionInfo>
    {
        public BuyerAuctionInfo Convert(Auction entity) => mapping[entity.Id];
    }
    
    [Fact]
    public async Task Info_Returns_Info_About_Auctions_Async()
    {
        // Arrange
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var infoDictionary = new Dictionary<AuctionId, BuyerAuctionInfo>
        {
            [new AuctionId(5)] = new DutchAuctionInfo
            {
                CurrentPrice = 123,
                AuctionId = new AuctionId(5),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending
            },
            
            [new AuctionId(2)] = new EnglishAuctionInfo
            {
                AuctionId = new AuctionId(2),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending,
                CurrentMinimumBid = 34
            },
            
            [new AuctionId(1)] = new FirstPriceSealedBidInfo
            {
                AuctionId = new AuctionId(1),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending
            }
        };
        
        var converter = new MockAuctionToBuyerAuctionInfoConverter(infoDictionary);
        
        AuctionId[] requestedIds = [new(5), new(2), new(1)];
        
        // Act
        var response = await BuyMethods.GetInfoAsync(requestedIds, mockAuctionRepo, converter);
        var result = Assert.IsType<Ok<IEnumerable<BuyerAuctionInfo>>>(response.Result).Value.ToList();
        
        // Assert
        Assert.Equal(3, result.Count);

        for (var i = 0; i < 3; i++)
        {
            var id = requestedIds[i];

            var expected = infoDictionary[id];
            var obtained = result[i];
            
            Assert.Equal(expected, obtained);
        }
    }
    
    [Fact]
    public async Task Info_Returns_Info_About_Auctions_Different_Request_Permutation_Ids_Order_Correct_Async()
    {
        // Arrange
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var infoDictionary = new Dictionary<AuctionId, BuyerAuctionInfo>
        {
            [new AuctionId(5)] = new DutchAuctionInfo
            {
                CurrentPrice = 123,
                AuctionId = new AuctionId(5),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending
            },

            [new AuctionId(2)] = new EnglishAuctionInfo
            {
                AuctionId = new AuctionId(2),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending,
                CurrentMinimumBid = 34
            },

            [new AuctionId(1)] = new FirstPriceSealedBidInfo
            {
                AuctionId = new AuctionId(1),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending
            }
        };
        
        var converter = new MockAuctionToBuyerAuctionInfoConverter(infoDictionary);
        
        AuctionId[] requestedIds = [new(2), new(5), new(1)];
        
        var response = await BuyMethods.GetInfoAsync(requestedIds, mockAuctionRepo, converter);
        var result = Assert.IsType<Ok<IEnumerable<BuyerAuctionInfo>>>(response.Result).Value.ToList();

        Assert.Equal(3, result.Count);

        for (int i = 0; i < 3; i++)
        {
            var id = requestedIds[i];

            var expected = infoDictionary[id];
            var obtained = result[i];
            
            Assert.Equal(expected, obtained);
        }
    }

    [Fact]
    public async Task Info_Returns_Not_Found_When_AuctionId_Nonexistent_Async()
    {
        // Arrange
        
        var infoDictionary = new Dictionary<AuctionId, BuyerAuctionInfo>
        {
            [new AuctionId(5)] = new DutchAuctionInfo
            {
                CurrentPrice = 123,
                AuctionId = new AuctionId(444),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending
            },
            
            [new AuctionId(2)] = new EnglishAuctionInfo
            {
                AuctionId = new AuctionId(444),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending,
                CurrentMinimumBid = 34
            },
            
            [new AuctionId(1)] = new FirstPriceSealedBidInfo
            {
                AuctionId = new AuctionId(444),
                Description = "something",
                SellerId = new UserId(123456789),
                StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Status = AuctionStatus.Pending
            }
        };
        
        var converter = new MockAuctionToBuyerAuctionInfoConverter(infoDictionary);
        
        AuctionId[] requestedIds = [new(5), new(2000), new(1)];
        
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);
        
        // Act
        var response = await BuyMethods.GetInfoAsync(requestedIds, mockAuctionRepo, converter);
        
        // Assert
        Assert.IsType<NotFound<AuctionIdNotFound>>(response.Result);
    }
}