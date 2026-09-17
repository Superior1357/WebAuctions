using JetBrains.Annotations;
using UnitTests.HelperClasses;
using Backend;
using Backend.AuctionEvaluation;
using Backend.Data.DataModel.Business;
using Backend.Data.SharedDataProperties;
using Backend.Utils;

namespace UnitTests.Tests;

[TestSubject(typeof(AuctionEvaluator))]
public class AuctionEvaluatorTest
{
    private readonly ICurrentTime _currentTime = new MockCurrentTime(new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero));
    private AuctionEvaluator Evaluator => new(_currentTime);

    private static User MakeUser(int id) => User.Restore(new UserId(id));

    private static Bid MakeBid(int id, decimal value, User bidder, DateTimeOffset time) =>
        Bid.Restore(new BidId(id), bidder, value, time);

    private static TestAddOnlyCollection<Bid> NoBids => new([]);

    [Fact]
    public void EvaluateStatus_Maps_English_Active_Status_Correctly()
    {
        var auction = EnglishAuction.Restore(
            new AuctionId(0),
            "English auction",
            MakeUser(0),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>(
                [
                    MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                    MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                ]),
            1000
        );

        Assert.Equal(AuctionStatus.Active, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_English_Finished_Status_Correctly()
    {
        var auction = EnglishAuction.Restore(
            new AuctionId(1),
            "English auction",
            MakeUser(0),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>(
                [
                    MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                    MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                ]),
            1000
        );

        Assert.Equal(AuctionStatus.Finished, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_English_Pending_Status_Correctly()
    {
        var auction = EnglishAuction.Restore(
            new AuctionId(2),
            "English auction",
            MakeUser(0),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2025, 3, 1, 1, 1, 1, TimeSpan.Zero),
            NoBids,
            1000
        );

        Assert.Equal(AuctionStatus.Pending, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_Dutch_Finished_After_Bid_Status_Correctly()
    {
        var auction = DutchAuction.Restore(
            new AuctionId(3),
            "Dutch auction",
            MakeUser(0),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
            1000,
            2,
            new TimeSpan(0, 1, 0)
        );

        Assert.Equal(AuctionStatus.Finished, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_Dutch_Finished_After_Time_End_Status_Correctly()
    {
        var auction = DutchAuction.Restore(
            new AuctionId(4),
            "Dutch auction",
            MakeUser(0),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
            null,
            1000,
            2,
            new TimeSpan(0, 1, 0)
        );

        Assert.Equal(AuctionStatus.Finished, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_Dutch_Pending_Status_Correctly()
    {
        var auction = DutchAuction.Restore(
            new AuctionId(5),
            "Dutch auction",
            MakeUser(0),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
            null,
            1000,
            2,
            new TimeSpan(0, 1, 0)
        );

        Assert.Equal(AuctionStatus.Pending, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_Dutch_Active_Status_Correctly()
    {
        var auction = DutchAuction.Restore(
            new AuctionId(6),
            "Dutch auction",
            MakeUser(0),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
            null,
            1000,
            2,
            new TimeSpan(0, 1, 0)
        );

        Assert.Equal(AuctionStatus.Active, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_First_Time_Sealed_Bid_Finished_Status_Correctly()
    {
        var auction = FirstPriceSealedBidAuction.Restore(
            new AuctionId(7),
            "English auction",
            MakeUser(0),
            NoBids,
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero)
        );

        Assert.Equal(AuctionStatus.Finished, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_First_Time_Sealed_Bid_Active_Status_Correctly()
    {
        var auction = FirstPriceSealedBidAuction.Restore(
            new AuctionId(8),
            "English auction",
            MakeUser(0),
            new TestAddOnlyCollection<Bid>(
                [
                    MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                    MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                ]),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 3, 1, 1, 1, 1, TimeSpan.Zero)
        );

        Assert.Equal(AuctionStatus.Active, Evaluator.EvaluateStatus(auction));
    }

    [Fact]
    public void EvaluateStatus_Maps_First_Time_Sealed_Bid_Pending_Status_Correctly()
    {
        var auction = FirstPriceSealedBidAuction.Restore(
            new AuctionId(9),
            "English auction",
            MakeUser(0),
            NoBids,
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 3, 1, 1, 1, 1, TimeSpan.Zero)
        );

        Assert.Equal(AuctionStatus.Pending, Evaluator.EvaluateStatus(auction));
    }

    public static IEnumerable<object[]> UnfinishedAuctions =>
        new List<Auction[]>
        {
            new Auction[]
            {
                EnglishAuction.Restore(
                    new AuctionId(0),
                    "English auction",
                    MakeUser(0),
                    new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new TestAddOnlyCollection<Bid>(
                    [
                        MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                        MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                    ]),
                    1000
                )
            },
            new Auction[]
            {
                EnglishAuction.Restore(
                    new AuctionId(2),
                    "English auction",
                    MakeUser(0),
                    new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2025, 3, 1, 1, 1, 1, TimeSpan.Zero),
                    NoBids,
                    1000
                )
            },
            
            new Auction[]
            {
                DutchAuction.Restore(
                    new AuctionId(5),
                    "Dutch auction",
                    MakeUser(0),
                    new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    null,
                    1000,
                    2,
                    new TimeSpan(0, 1, 0)
                )
            },
            
            new Auction[]
            {
                DutchAuction.Restore(
                new AuctionId(4),
                "Dutch auction",
                MakeUser(0),
                new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                null,
                1000,
                2,
                new TimeSpan(0, 1, 0))
            },
            
            new Auction[]
            {
                DutchAuction.Restore(
                new AuctionId(5),
                "Dutch auction",
                MakeUser(0),
                new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
                null,
                1000,
                2,
                new TimeSpan(0, 1, 0))
            },
            
            new Auction[]
            {
                DutchAuction.Restore(
                    new AuctionId(6),
                    "Dutch auction",
                    MakeUser(0),
                    new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    null,
                    1000,
                    2,
                    new TimeSpan(0, 1, 0)
                )
            },
            
            new Auction[]
            {
                FirstPriceSealedBidAuction.Restore(
                    new AuctionId(8),
                    "English auction",
                    MakeUser(0),
                    new TestAddOnlyCollection<Bid>(
                    [
                        MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                        MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                    ]),
                    new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2024, 3, 1, 1, 1, 1, TimeSpan.Zero)),
            },
            
            new Auction[]
            {
                FirstPriceSealedBidAuction.Restore(
                    new AuctionId(9),
                    "English auction",
                    MakeUser(0),
                    NoBids,
                    new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2024, 3, 1, 1, 1, 1, TimeSpan.Zero)
                )
            }
        };

    [Theory]
    [MemberData(nameof(UnfinishedAuctions))]
    internal void EvaluateWinner_Returns_Null_When_Auction_Unfinished(Auction unfinishedAuction)
    {
        var winner = Evaluator.EvaluateWinnerInfo(unfinishedAuction);
        Assert.Null(winner);
    }

    public static IEnumerable<object[]> FinishedNoBidAuctions =>
        new List<Auction[]>
        {
            new Auction[]
            {
                EnglishAuction.Restore(
                    new AuctionId(1),
                    "English auction",
                    MakeUser(0),
                    new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero),
                    NoBids,
                    1000
                )
            },
            
            new Auction[]
            {
                DutchAuction.Restore(
                    new AuctionId(4),
                    "Dutch auction",
                    MakeUser(0),
                    new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                    null,
                    1000,
                    2,
                    new TimeSpan(0, 1, 0)
                )
            },
            
            new Auction[]
            {
                FirstPriceSealedBidAuction.Restore(
                    new AuctionId(7),
                    "English auction",
                    MakeUser(0),
                    NoBids,
                    new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                    new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero)
                )
            }
        };
    
    [Theory]
    [MemberData(nameof(FinishedNoBidAuctions))]
    internal void EvaluateWinner_Returns_Null_When_Finished_Auction_No_Bid(Auction finishedNoBidAuction)
    {
        var winner = Evaluator.EvaluateWinnerInfo(finishedNoBidAuction);
        Assert.Null(winner);
    }

    public static IEnumerable<object[]> FinishedWithWinnersAuctions
    {
        get
        {
            var winner1 = MakeUser(2);
            var winner2 = MakeUser(1);
            var winner3 = MakeUser(2);
            
            return new List<object[]>
                {
                    new object[]
                    {
                        EnglishAuction.Restore(
                            new AuctionId(1),
                            "English auction",
                            MakeUser(0),
                            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                            new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero),
                            new TestAddOnlyCollection<Bid>(
                            [
                                MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                                MakeBid(1, 2000, winner1, new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                            ]),
                            1000
                        ),
                        new WinnerInfo(2000, winner1.Id)
                    },
                    
                    new object[]
                    {
                        DutchAuction.Restore(
                            new AuctionId(3),
                            "Dutch auction",
                            MakeUser(0),
                            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                            MakeBid(0, 1000, winner2, new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                            1000,
                            2,
                            new TimeSpan(0, 1, 0)
                        ),
                        new WinnerInfo(1000, winner2.Id)
                    },
                    
                    new object[]
                     {
                         FirstPriceSealedBidAuction.Restore(
                             new AuctionId(8),
                             "English auction",
                             MakeUser(0),
                             new TestAddOnlyCollection<Bid>(
                             [
                                 MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                                 MakeBid(1, 2000, winner3, new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                             ]),
                             new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                             new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero)
                         ),
                         new WinnerInfo(2000, winner3.Id)
                     }
                };
        }
    }
    
    [Theory]
    [MemberData(nameof(FinishedWithWinnersAuctions))]
    internal void EvaluateWinner_Returns_User_With_Highest_Bid_Dutch_English_FirstPriceSealedBid(Auction finishedAuction, WinnerInfo? expectedWinnerInfo)
    {
        var user = Evaluator.EvaluateWinnerInfo(finishedAuction);
        Assert.Equal(expectedWinnerInfo, user);
    }
    
    public static IEnumerable<object[]> FinishedAuctions
    {
        get
        {
            var winner2 = MakeUser(1);
            var winner3 = MakeUser(2);
            
            return new List<object[]>
                {
                    new object[]
                    {
                        EnglishAuction.Restore(
                            new AuctionId(1),
                            "English auction",
                            MakeUser(0),
                            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                            new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero),
                            new TestAddOnlyCollection<Bid>(
                            [
                            ]),
                            1000
                        )
                    },
                    
                    new object[]
                    {
                        DutchAuction.Restore(
                            new AuctionId(3),
                            "Dutch auction",
                            MakeUser(0),
                            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                            MakeBid(0, 1000, winner2, new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                            1000,
                            2,
                            new TimeSpan(0, 1, 0)
                        )
                    },
                    
                    new object[]
                     {
                         FirstPriceSealedBidAuction.Restore(
                             new AuctionId(8),
                             "English auction",
                             MakeUser(0),
                             new TestAddOnlyCollection<Bid>(
                             [
                                 MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                                 MakeBid(1, 2000, winner3, new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                             ]),
                             new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                             new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero)
                         )
                     }
                };
        }
    }
    
    [Theory]
    [MemberData(nameof(FinishedAuctions))]
    internal void EvaluateCurrentMinimumBidValue_Returns_Null_When_Auction_Finished(Auction finishedAuction)
    {
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(finishedAuction);
        Assert.Null(evaluation);
    }

    [Fact]
    public void EvaluateCurrentMinimumBidValue_Returns_Starting_Price_Auction_Dutch_Pending()
    {
        var dutchAuction = DutchAuction.Restore(
            new AuctionId(5),
            "Dutch auction",
            MakeUser(0),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
            null,
            1000,
            2,
            new TimeSpan(0, 1, 0)
        );
        
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(dutchAuction);
        Assert.Equal(1000, evaluation);
    }
    
    [Fact]
    public void EvaluateCurrentMinimumBidValue_Returns_Starting_Price_Auction_English_Pending()
    {
        var englishAuction = EnglishAuction.Restore(
            new AuctionId(5),
            "English auction",
            MakeUser(0),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>([]),
            1000
        );
        
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(englishAuction);
        Assert.Equal(1000, evaluation);
    }
    
    [Fact]
    public void EvaluateCurrentMinimumBidValue_Returns_Null_Auction_First_Price_Sealed_Bid_Pending()
    {
        var firstPriceSealedBidAuction = FirstPriceSealedBidAuction.Restore(
            new AuctionId(5),
            "first price sealed bid auction",
            MakeUser(0),
            new TestAddOnlyCollection<Bid>([]),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero)
        );
        
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(firstPriceSealedBidAuction);
        Assert.Null(evaluation);
    }

    [Fact]
    public void EvaluateCurrentMinimumBidValue_Returns_Last_Bid_Plus_One_English_Auction_Active()
    {
        var englishActive = EnglishAuction.Restore(
            new AuctionId(1),
            "English auction",
            MakeUser(0),
            new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new TestAddOnlyCollection<Bid>(
                [
                    MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                    MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                ]
            ),
            1000);
        
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(englishActive);
        Assert.Equal(2001, evaluation);
    }
    
    [Fact]
    public void EvaluateCurrentMinimumBidValue_Returns_Null_First_Price_Sealed_Bid_Active()
    {
        var firstPriceSealedBidActive = FirstPriceSealedBidAuction.Restore(
                new AuctionId(1),
                "first price sealed bid auction",
                MakeUser(0),
                new TestAddOnlyCollection<Bid>(
                    [
                        MakeBid(0, 1000, MakeUser(1), new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)),
                        MakeBid(1, 2000, MakeUser(2), new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero))
                    ]
                ),
                new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero));
        
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(firstPriceSealedBidActive);
        Assert.Null(evaluation);
    }
    
    [Fact]
    public void EvaluateCurrentMinimumBidValue_Returns_Time_Calculated_Price_Active()
    {
        var dutchAuction = DutchAuction.Restore(
                new AuctionId(1),
                "Dutch auction",
                MakeUser(0),
                new DateTimeOffset(2021, 12, 24, 5, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                null, 
                1000,
                3,
                new TimeSpan(2,0,0,0)
            );
        
        var evaluation = Evaluator.EvaluateCurrentMinimumBidValue(dutchAuction);
        Assert.Equal(991, evaluation);
    }
}