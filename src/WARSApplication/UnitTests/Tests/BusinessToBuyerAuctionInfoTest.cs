using JetBrains.Annotations;
using UnitTests.HelperClasses;
using Backend.AuctionEvaluation;
using Backend.Data.APIData.Outgoing;
using Backend.Data.BusinessModel.Business;
using Backend.Data.DataConverters;
using Backend.Data.SharedDataProperties;

namespace UnitTests.Tests;

[TestSubject(typeof(BusinessToBuyerAuctionInfo))]
public class BusinessToBuyerAuctionInfoTest
{
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

    private static IEnumerable<BuyerAuctionInfo> ExpectedInfoData =>
    [
        new EnglishAuctionInfo
        {
            AuctionId = new AuctionId(0),
            Description = "added_before1",
            SellerId = new UserId(TestUser.Id.Id + 1),
            StartTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Status = AuctionStatus.Pending,
            CurrentMinimumBid = 1000
        },
        new EnglishAuctionInfo
        {
            AuctionId = new AuctionId(1),
            Description = "added_before3",
            SellerId = TestUser.Id,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Status = AuctionStatus.Pending,
            CurrentMinimumBid = 1000
        },
        new DutchAuctionInfo
        {
            AuctionId = new AuctionId(2),
            Description = "added_before2",
            SellerId = TestUser.Id,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Status = AuctionStatus.Pending,
            CurrentPrice = 1000
        },
        new EnglishAuctionInfo
        {
            AuctionId = new AuctionId(3),
            Description = "added_before_active_1",
            SellerId = new UserId(TestUser.Id.Id + 1),
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Status = AuctionStatus.Active,
            CurrentMinimumBid = 2001
        },
        new FirstPriceSealedBidInfo
        {
            AuctionId = new AuctionId(4),
            Description = "added_before_active_2",
            SellerId = new UserId(TestUser.Id.Id + 2),
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2021, 5, 1, 0, 0, 0, TimeSpan.Zero),
            Status = AuctionStatus.Finished
        },
        new FirstPriceSealedBidInfo
        {
            AuctionId = new AuctionId(5),
            Description = "added_before_active_3",
            SellerId = new UserId(TestUser.Id.Id + 2),
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Status = AuctionStatus.Active
        }
    ];
    
    public static IEnumerable<object[]> TestedContent => MockAuctionsRepoContent
            .Zip(ExpectedInfoData, (auction, expected) => new object[] { auction, expected });
    
    [Theory]
    [MemberData(nameof(TestedContent))]
    internal void Conversion_Works(Auction auction, BuyerAuctionInfo expectedInfo)
    {
        // Arrange
        var testedConverter = new BusinessToBuyerAuctionInfo(MockAuctionEvaluator);
        
        // Act
        var obtainedInfo = testedConverter.Convert(auction);
        
        // Assert
        Assert.Equal(expectedInfo, obtainedInfo);
    }
}