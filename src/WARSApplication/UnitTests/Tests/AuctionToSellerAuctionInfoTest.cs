using JetBrains.Annotations;
using WARSApplication;
using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataConverters;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace UnitTests.Tests;

[TestSubject(typeof(AuctionToSellerAuctionInfo))]
public class AuctionToSellerAuctionInfoTest
{
    private class LocalMockEvaluator(AuctionStatus status, WinnerInfo? winnerInfo, Price? price) : IAuctionEvaluator
    {
        public AuctionStatus EvaluateStatus(Auction auction) => status;
        public WinnerInfo? EvaluateWinner(Auction auction) => winnerInfo;
        public Price? EvaluateCurrentMinimumBidValue(Auction auction) => price;
    }
    
    public static TheoryData<Auction, SellerAuctionInfo>
        AuctionToSellerAuctionInfoConvertReturnsCorrectSellerAuctionInfoData => new()
        {
            {
                EnglishAuction.Restore(
                    new AuctionId(0),
                    "English auction",
                    User.Restore(new UserId(100)),
                    new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new AddOnlyCollection<Bid>([]),
                    1000),
                
                new EnglishSellerAuctionInfo { 
                    Description = "English auction",
                    StartingPrice = 1000,
                    StartTime = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Id = new AuctionId(0),
                    Info = new PendingStatus()
                }
            },

            {
                DutchAuction.Restore(
                    new AuctionId(0),
                    "Dutch auction",
                    User.Restore(new UserId(100)),
                    new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    null,
                    1000,
                    10,
                    new TimeSpan(0, 1, 0)
                    ),
                
                new DutchSellerAuctionInfo
                {
                    Description = "Dutch auction",
                    StartingPrice = 1000,
                    StartTime = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Id = new AuctionId(0),
                    Info = new PendingStatus(),
                    PriceDecrementRate = 10,
                    PriceDecrementFrequency = new TimeSpan(0, 1, 0)
                }
            },

            {
                FirstPriceSealedBidAuction.Restore(
                        new AuctionId(0),
                        "First price sealed bid",
                        User.Restore(new UserId(100)),
                        new AddOnlyCollection<Bid>([]),
                        new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
                        new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    ),
                
                new FirstPriceSealedBidSellerAuctionInfo
                {
                    Id = new AuctionId(0),
                    Description = "First price sealed bid",
                    StartTime = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    Info = new PendingStatus()
                }
            }
        };
    
    [Fact]
    public void Static_Info_English_Auction_Converted_Correctly()
    {
        // Arrange
        var auction = EnglishAuction.Restore(
            new AuctionId(0),
            "English auction",
            User.Restore(new UserId(100)),
            new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new AddOnlyCollection<Bid>([]),
            1000);

        var expectedInfo = new EnglishSellerAuctionInfo
        {
            Description = "English auction",
            StartingPrice = 1000,
            StartTime = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Id = new AuctionId(0),
            Info = new PendingStatus()
        };
        
        var converter = new AuctionToSellerAuctionInfo(
                new LocalMockEvaluator(AuctionStatus.Pending, null, null)
            );
        
        // Act
        var actualSellerAuctionInfo = converter.Convert(auction);
        
        // Assert
        var actualInfo = Assert.IsType<EnglishSellerAuctionInfo>(actualSellerAuctionInfo);
        Assert.Equal(expectedInfo, actualInfo);
    }
    
    [Fact]
    public void Static_Info_Dutch_Auction_Converted_Correctly()
    {
        // Arrange
        var auction = DutchAuction.Restore(
            new AuctionId(0),
            "Dutch auction",
            User.Restore(new UserId(100)),
            new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            null,
            1000,
            10,
            new TimeSpan(0, 1, 0)
        );

        var expectedInfo = new DutchSellerAuctionInfo
        {
            Description = "Dutch auction",
            StartingPrice = 1000,
            StartTime = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Id = new AuctionId(0),
            Info = new PendingStatus(),
            PriceDecrementRate = 10,
            PriceDecrementFrequency = new TimeSpan(0, 1, 0)
        };
        
        var converter = new AuctionToSellerAuctionInfo(
            new LocalMockEvaluator(AuctionStatus.Pending, null, null)
            );
        
        // Act
        var actualSellerAuctionInfo = converter.Convert(auction);
        
        // Assert
        var actualInfo = Assert.IsType<DutchSellerAuctionInfo>(actualSellerAuctionInfo);
        Assert.Equal(expectedInfo, actualInfo);
    }

    [Fact]
    public void Static_Info_FirstPriceSealedBid_Auction_Converted_Correctly()
    {
        // Arrange
        var auction = FirstPriceSealedBidAuction.Restore(
            new AuctionId(0),
            "First price sealed bid",
            User.Restore(new UserId(100)),
            new AddOnlyCollection<Bid>([]),
            new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero)
        );

        var expectedInfo = new FirstPriceSealedBidSellerAuctionInfo
        {
            Id = new AuctionId(0),
            Description = "First price sealed bid",
            StartTime = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Info = new PendingStatus()
        };
        
        var converter = new AuctionToSellerAuctionInfo(new LocalMockEvaluator(AuctionStatus.Pending, null, null));
        
        // Act
        var actualSellerAuctionInfo = converter.Convert(auction);
        
        // Assert
        var actualInfo = Assert.IsType<FirstPriceSealedBidSellerAuctionInfo>(actualSellerAuctionInfo);
        Assert.Equal(expectedInfo, actualInfo);
    }

    public static TheoryData<IAuctionEvaluator, StatusSpecificInfo> EvaluatorAndExpectedOutput = new()
    {
        {
            new LocalMockEvaluator(AuctionStatus.Pending, null, null),
            new PendingStatus()
        },

        {
            new LocalMockEvaluator(AuctionStatus.Active, null, null),
            new ActiveStatus()
        },

        {
            new LocalMockEvaluator(AuctionStatus.Finished, null, null),
            new FinishedStatus
            {
                Winner = null
            }
        },

        {
            new LocalMockEvaluator(AuctionStatus.Finished,
                new WinnerInfo(1000, new UserId(2)), 1000),
            new FinishedStatus
            {
                Winner = new WinnerInfo(1000, new UserId(2))
            }
        }
    };
    
    [Theory]
    [MemberData(nameof(EvaluatorAndExpectedOutput))]
    public void Dynamic_Info_English_Auction_Read_Correctly(IAuctionEvaluator auctionEvaluator, StatusSpecificInfo expectedInfo) // TODO: just test whether whatever it gets from the status evaluator is correct
    {
        // Arrange
        var auction = EnglishAuction.Restore(
            new AuctionId(0),
            "English auction",
            User.Restore(new UserId(100)),
            new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new AddOnlyCollection<Bid>([]),
            1000);
        
        // Act
        var result = new AuctionToSellerAuctionInfo(auctionEvaluator).Convert(auction);
        
        // Assert
        Assert.Equal(expectedInfo, result.Info);
    }
    
    [Theory]
    [MemberData(nameof(EvaluatorAndExpectedOutput))]
    public void Dynamic_Info_Dutch_Auction_Read_Correctly(IAuctionEvaluator auctionEvaluator, StatusSpecificInfo expectedInfo) // TODO: just test whether whatever it gets from the status evaluator is correct
    {
        // Arrange
        var auction = DutchAuction.Restore(
            new AuctionId(0),
            "Dutch auction",
            User.Restore(new UserId(100)),
            new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            null,
            1000,
            10,
            new TimeSpan(0, 1, 0)
        );
        
        // Act
        var result = new AuctionToSellerAuctionInfo(auctionEvaluator).Convert(auction);
        
        // Assert
        Assert.Equal(expectedInfo, result.Info);
    }
    
    [Theory]
    [MemberData(nameof(EvaluatorAndExpectedOutput))]
    public void Dynamic_Info_FirstPriceSealedBid_Auction_Read_Correctly(IAuctionEvaluator auctionEvaluator, StatusSpecificInfo expectedInfo) // TODO: just test whether whatever it gets from the status evaluator is correct
    {
        // Arrange
        var auction = FirstPriceSealedBidAuction.Restore(
            new AuctionId(0),
            "First price sealed bid",
            User.Restore(new UserId(100)),
            new AddOnlyCollection<Bid>([]),
            new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero)
        );
        
        // Act
        var result = new AuctionToSellerAuctionInfo(auctionEvaluator).Convert(auction);
        
        // Assert
        Assert.Equal(expectedInfo, result.Info);
    }
}