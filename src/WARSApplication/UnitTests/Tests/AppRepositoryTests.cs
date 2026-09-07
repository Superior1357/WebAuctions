using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using WARSApplication.Data.DataModel;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Repository;

namespace UnitTests.Tests;
[TestSubject(typeof(AppRepository))]
public class AppRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AuctionDbContext> _contextOptions;

    private readonly TimeProvider _fakeTimeProvider =
        new FakeTimeProvider(new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero));

    private static (List<DataModelUser>, List<DataModelAuction>) _databaseInitialContent()
    {
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            new()
            {
                IdentityId = "1"
            },
            new()
            {
                IdentityId = "2"
            }
        };

        var lastBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };

        List<DataModelAuction> auctions =
        [

            new EnglishDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                Bids = new List<DataModelBid>
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    },

                    lastBid
                }
            },
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
            },
            new FirstPriceSealedBidDataModelAuction
            {
                Description = "First time sealed bid",
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
                Seller = users[2]
            }
        ];

        return (users, auctions);
    }
    
    public AppRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AuctionDbContext(_contextOptions);
        context.Database.EnsureCreated();
    }

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

    private static void AssertBidsEquivalency(IEnumerable<DataModelBid> expectedBids, IEnumerable<Bid> actualBids)
    {
        var bids = expectedBids.ToList();
        var retrievedBids = actualBids.ToList();

        Assert.Equal(bids.Count, retrievedBids.Count);

        for (int j = 0; j < bids.Count; j++)
        {
            var dataBid = bids[j];
            var retrievedBid = retrievedBids[j];

            Assert.Equal(dataBid.Bidder.Id, retrievedBid.Bidder.Id);
            Assert.Equal(dataBid.Value, retrievedBid.Value);
            Assert.Equal(dataBid.Timestamp, retrievedBid.Time);
            Assert.Equal(dataBid.Id, retrievedBid.Id);
        }
    }
    
    private static void AssertDataModelBidsMatch(DataModelBid expected, DataModelBid actual)
    {
        Assert.Equal(expected.Bidder, actual.Bidder);
        Assert.Equal(expected.Value, actual.Value);
        Assert.Equal(expected.Timestamp, actual.Timestamp);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Bidder.Id, actual.Bidder.Id);
        Assert.Equal(expected.AuctionId, actual.AuctionId);
    }
    
    private static void AssertDataModelBusinessModelAuctionsMatch(DataModelAuction dataAuction, Auction businessAuction)
    {
        Assert.Equal(dataAuction.Id, businessAuction.Id);
        Assert.Equal(dataAuction.Description, businessAuction.Description);
        Assert.Equal(dataAuction.StartTime, businessAuction.StartTime);
        Assert.Equal(dataAuction.EndTime, businessAuction.EndTime);
        Assert.Equal(dataAuction.Seller.Id, businessAuction.Seller.Id);

        switch (dataAuction)
        {
            case EnglishDataModelAuction englishAuction:
                var englishSpecificData = Assert.IsType<EnglishAuction>(businessAuction);
                Assert.Equal(englishAuction.StartingPrice, englishSpecificData.StartingPrice);
                AssertBidsEquivalency(dataAuction.Bids, englishSpecificData.Bids);
                break;
            case DutchDataModelAuction dutchAuction:
                var dutchSpecificData = Assert.IsType<DutchAuction>(businessAuction);
                Assert.Equal(dutchAuction.StartingPrice, dutchSpecificData.StartingPrice);
                Assert.Equal(dutchAuction.PriceDecrementFrequency, dutchSpecificData.PriceDecrementFrequency);
                Assert.Equal(dutchAuction.PriceDecrementRate, dutchSpecificData.PriceDecrementRate);

                if (dataAuction.Bids.Count != 0)
                {
                    Assert.NotNull(dutchSpecificData.Bid);
                    AssertBidsEquivalency(dataAuction.Bids, [dutchSpecificData.Bid]);
                }
                else
                {
                    Assert.Null(dutchSpecificData.Bid);
                }

                break;
            case FirstPriceSealedBidDataModelAuction:
                var businessAuctionData = Assert.IsType<FirstPriceSealedBidAuction>(businessAuction);
                AssertBidsEquivalency(dataAuction.Bids, businessAuctionData.Bids);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


    }

    private static void AssertDataModelAuctionsMatch(DataModelAuction expected, DataModelAuction actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.StartTime, actual.StartTime);
        Assert.Equal(expected.EndTime, actual.EndTime);
        Assert.Equal(expected.Seller, actual.Seller);

        switch (expected)
        {
            case EnglishDataModelAuction englishAuction:
                var englishSpecificData = Assert.IsType<EnglishDataModelAuction>(actual);
                Assert.Equal(englishAuction.StartingPrice, englishSpecificData.StartingPrice);
                break;
            case DutchDataModelAuction dutchAuction:
                var dutchSpecificData = Assert.IsType<DutchDataModelAuction>(actual);
                Assert.Equal(dutchAuction.StartingPrice, dutchSpecificData.StartingPrice);
                Assert.Equal(dutchAuction.PriceDecrementFrequency, dutchSpecificData.PriceDecrementFrequency);
                Assert.Equal(dutchAuction.PriceDecrementRate, dutchSpecificData.PriceDecrementRate);
                break;
            case FirstPriceSealedBidDataModelAuction:
                Assert.IsType<FirstPriceSealedBidDataModelAuction>(actual);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var expectedBids = expected.Bids.ToList();
        var actualBids = actual.Bids.ToList();

        Assert.Equal(expectedBids.Count, actualBids.Count);

        for (int j = 0; j < expectedBids.Count; j++)
        {
            var expectedBid = expectedBids[j];
            var actualBid = actualBids[j];

            Assert.Equal(expectedBid.Bidder.Id, actualBid.Bidder.Id);
            Assert.Equal(expectedBid.Value, actualBid.Value);
            Assert.Equal(expectedBid.Timestamp, actualBid.Timestamp);
            Assert.Equal(expectedBid.Id, actualBid.Id);
        }
    }

    [Fact]
    public async Task WhereListAsyncMapsAuctionsCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            new()
            {
                IdentityId = "1"
            },
            new()
            {
                IdentityId = "2"
            }
        };

        var lastBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };


        var dataAuctions = new List<DataModelAuction>
        {
            new EnglishDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                Bids = new List<DataModelBid>
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    },

                    lastBid
                }
            },
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
            },
            new FirstPriceSealedBidDataModelAuction
            {
                Description = "First time sealed bid",
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
                Seller = users[2]
            }
        };

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        List<Auction> retrievedAuctions;
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }

        // Assert
        dataAuctions = dataAuctions.OrderBy(dataAuction => dataAuction.Id.Id).ToList();
        Assert.Equal(3, retrievedAuctions.Count);
        for (int i = 0; i < 3; i++)
        {
            var dataAuction = dataAuctions[i];
            var retrievedAuction = retrievedAuctions[i];
            AssertDataModelBusinessModelAuctionsMatch(dataAuction, retrievedAuction);
        }
    }

    [Fact]
    public async Task AddAsyncAddsAuctionWhenCommitCalled()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            new()
            {
                IdentityId = "1"
            },
            new()
            {
                IdentityId = "2"
            }
        };

        var lastBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };


        var dataAuctions = new List<DataModelAuction>
        {
            new EnglishDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                Bids = new List<DataModelBid>
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    },

                    lastBid
                }
            },
            
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
            },
            
            new FirstPriceSealedBidDataModelAuction
            {
                Description = "First time sealed bid",
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
                Seller = users[2]
            }
        };

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        var addedAuction = new DutchAuction(_fakeTimeProvider.GetLocalNow(),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero))
        {
            Description = "new dutch",
            PriceDecrementFrequency = new TimeSpan(0, 1, 0),
            PriceDecrementRate = 1,
            StartingPrice = 1000,
            Seller = User.Restore(users[2].Id)
        };
        
        // Act
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            await repo.AddAsync(addedAuction);
            await repo.CommitAsync();
        }

        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var auctions = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder).Include(a => a.Seller)
                .ToList();
            dataAuctions = dataAuctions.OrderBy(dataAuction => dataAuction.Id.Id).ToList();

            Assert.Equal(4, auctions.Count);
            for (int i = 0; i < 3; i++)
                AssertDataModelAuctionsMatch(dataAuctions[i], auctions[i]);

            var expectedRespectiveData = new DutchDataModelAuction
            {
                Id = auctions[3].Id,
                Description = addedAuction.Description,
                StartTime = addedAuction.StartTime,
                EndTime = addedAuction.EndTime,
                Seller = users[2],
                StartingPrice = addedAuction.StartingPrice,
                PriceDecrementFrequency = addedAuction.PriceDecrementFrequency,
                PriceDecrementRate = addedAuction.PriceDecrementRate
            };

            AssertDataModelAuctionsMatch(expectedRespectiveData, auctions[3]);
        }
    }

    [Fact]
    public async Task AddAsyncDoesNotAddAuctionWhenCommitNotCalled()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            new()
            {
                IdentityId = "1"
            },
            new()
            {
                IdentityId = "2"
            }
        };

        var lastBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };


        var dataAuctions = new List<DataModelAuction>
        {
            new EnglishDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                Bids = new List<DataModelBid>
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    },

                    lastBid
                }
            },
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
            },
            new FirstPriceSealedBidDataModelAuction
            {
                Description = "First time sealed bid",
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
                Seller = users[2]
            }
        };

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        var addedAuction = new DutchAuction(_fakeTimeProvider.GetLocalNow(),
            new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero))
        {
            Description = "new dutch",
            PriceDecrementFrequency = new TimeSpan(0, 1, 0),
            PriceDecrementRate = 1,
            StartingPrice = 1000,
            Seller = User.Restore(users[2].Id)
        };

        var expectedRespectiveData = new DutchDataModelAuction
        {
            Description = addedAuction.Description,
            StartTime = addedAuction.StartTime,
            EndTime = addedAuction.EndTime,
            Seller = users[2],
            StartingPrice = addedAuction.StartingPrice,
            PriceDecrementFrequency = addedAuction.PriceDecrementFrequency,
            PriceDecrementRate = addedAuction.PriceDecrementRate
        };

        // Act
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            await repo.AddAsync(addedAuction);
        }
        
        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var auctions = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder).Include(a => a.Seller)
                .ToList();
            dataAuctions = dataAuctions.OrderBy(dataAuction => dataAuction.Id.Id).ToList();
            Assert.Equal(3, auctions.Count);
            for (int i = 0; i < 3; i++)
                AssertDataModelAuctionsMatch(dataAuctions[i], auctions[i]);
        }
    }
    
    [Fact]
    public async Task Update_Set_Endtime_Updates_Auction_Commit_Called_Async()
    {
        // Arrange
        var (users, dataAuctions) = _databaseInitialContent();

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        List<DataModelAuction> previousDatabaseContent;

        await using (var context = new AuctionDbContext(_contextOptions))
        {
            previousDatabaseContent = context.Auctions.ToList();

            var repo = new AppRepository(context);
            var retrievedAuctions = (await repo.ListAsync()).ToList();

            var updatedAuction = retrievedAuctions.First(a => a is FirstPriceSealedBidAuction);
            updatedAuction.EndTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);

            await repo.UpdateAsync(updatedAuction);
            await repo.CommitAsync();
        }

        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var updatedDatabaseContent = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller).ToList();
            Assert.Equal(3, updatedDatabaseContent.Count);

            for (int i = 0; i < 3; i++)
            {
                var previousDataAuction = previousDatabaseContent[i];
                var updatedDataAuction = updatedDatabaseContent[i];

                if (i == 2)
                {
                    var firstTimeSealedBidDataAuction =
                        Assert.IsType<FirstPriceSealedBidDataModelAuction>(updatedDataAuction);

                    Assert.Equal(previousDataAuction.Id, updatedDataAuction.Id);
                    Assert.Equal(firstTimeSealedBidDataAuction.Description, updatedDataAuction.Description);
                    Assert.Equal(firstTimeSealedBidDataAuction.StartTime, updatedDataAuction.StartTime);
                    Assert.Equal(new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero), updatedDataAuction.EndTime);
                    Assert.Equal(firstTimeSealedBidDataAuction.Seller, updatedDataAuction.Seller);
                }
                else
                    AssertDataModelAuctionsMatch(previousDataAuction, updatedDataAuction);
            }
        }
    }

    [Fact]
    public async Task Update_English_Auction_Updated_Bids_Adds_Bid_Commit_Called_Async()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            
            new()
            {
                IdentityId = "1"
            },
            
            new()
            {
                IdentityId = "2"
            }
        };

        var lastEnglishBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };

        var lastFirstPriceSealedBid = new DataModelBid
            {
                Bidder = users[1],
                Value = 500,
                Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
            };

        var englishDataModelAuction = new EnglishDataModelAuction
        {
            Description = "English auction",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Seller = users[0],
            StartingPrice = 1000,
            Bids = new List<DataModelBid>
            {
                lastEnglishBid
            }
        };

        var dutchDataModelAuction = new DutchDataModelAuction
        {
            Description = "Dutch auction",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Seller = users[0],
            StartingPrice = 1000,
            PriceDecrementFrequency = new TimeSpan(0, 1, 0),
            PriceDecrementRate = 2
        };

        var firstPriceSealedBidDataModelAuction = new FirstPriceSealedBidDataModelAuction
        {
            Description = "First time sealed bid",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
            Seller = users[2],
            Bids = new List<DataModelBid>
            {
                lastFirstPriceSealedBid
            }
        }; 
        
        List<DataModelAuction> dataAuctions =
        [
            englishDataModelAuction,
            dutchDataModelAuction,
            firstPriceSealedBidDataModelAuction
        ];

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        var bidder = User.Restore(users[0].Id);
        var addedBid = new Bid
        {
            Bidder = bidder,
            Time = new DateTimeOffset(2022, 1, 1, 1, 1, 2, TimeSpan.Zero),
            Value = 10_000
        };
        
        // Act
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            var retrievedAuctions = (await repo.ListAsync()).ToList();
            
            var updatedAuction = (EnglishAuction)retrievedAuctions.First(a => a is EnglishAuction);
            updatedAuction.Bids.Add(addedBid);
            
            await repo.UpdateAsync(updatedAuction);
            await repo.CommitAsync();
        }
        
        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var updatedBidsDatabaseContent = context.Bids.Include(dataModelBid => dataModelBid.Bidder).ToList();
            
            Assert.Equal(3, updatedBidsDatabaseContent.Count);
            Assert.Contains(updatedBidsDatabaseContent, b => b.Id == lastFirstPriceSealedBid.Id);
            Assert.Contains(updatedBidsDatabaseContent, b => b.Id == lastEnglishBid.Id);
            
            var lastFirstPriceSealedBidAfter = updatedBidsDatabaseContent.First(b => b.Id == lastFirstPriceSealedBid.Id);
            var lastEnglishBidAfter = updatedBidsDatabaseContent.First(b => b.Id == lastEnglishBid.Id);
            
            AssertDataModelBidsMatch(lastFirstPriceSealedBid, lastFirstPriceSealedBidAfter);
            AssertDataModelBidsMatch(lastEnglishBid, lastEnglishBidAfter);
            
            var newestEnglishBid = updatedBidsDatabaseContent.First(b => b.Value == 10_000);
            
            Assert.Equal(bidder.Id, newestEnglishBid.Bidder.Id);
            Assert.Equal(englishDataModelAuction.Id, newestEnglishBid.AuctionId);
            Assert.Equal(addedBid.Value, newestEnglishBid.Value);
            
            var updatedAuctionDatabaseContent = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller).ToList();

            Assert.Equal(3, updatedAuctionDatabaseContent.Count);

            var firstPriceSealedBidDataModelAuctionAfter =
                updatedAuctionDatabaseContent.First(a => a is FirstPriceSealedBidDataModelAuction);
            var dutchDataModelAuctionAfter = updatedAuctionDatabaseContent.First(a => a is DutchDataModelAuction);

            AssertDataModelAuctionsMatch(firstPriceSealedBidDataModelAuction, firstPriceSealedBidDataModelAuctionAfter);
            AssertDataModelAuctionsMatch(dutchDataModelAuction, dutchDataModelAuctionAfter);
            
            var englishDataModelAuctionAfter = updatedAuctionDatabaseContent.First(a => a is EnglishDataModelAuction);
            Assert.Equal(englishDataModelAuction.Id, englishDataModelAuctionAfter.Id);
            Assert.Equal(englishDataModelAuction.Description, englishDataModelAuctionAfter.Description);
            Assert.Equal(englishDataModelAuction.StartTime, englishDataModelAuctionAfter.StartTime);
            Assert.Equal(englishDataModelAuction.EndTime, englishDataModelAuctionAfter.EndTime);
            Assert.Equal(englishDataModelAuction.Seller, englishDataModelAuctionAfter.Seller);
            
            var updatedAuctionBids = englishDataModelAuctionAfter.Bids.ToList();
            Assert.Equal(2, updatedAuctionBids.Count);
            Assert.Contains(updatedAuctionBids, b => b.Id == lastEnglishBid.Id);
            
            var addedNewAfter = updatedAuctionBids.First(b => b.Id != lastEnglishBid.Id);
            
            Assert.Equal(addedBid.Value, addedNewAfter.Value);
            Assert.Equal(addedBid.Time, addedNewAfter.Timestamp);
            Assert.NotEqual(0, englishDataModelAuctionAfter.Version);
        }
    }
    
    [Fact]
    public async Task Update_Dutch_Auction_Updated_Bids_Adds_Bid_Commit_Called_Async()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            
            new()
            {
                IdentityId = "1"
            },
            
            new()
            {
                IdentityId = "2"
            }
        };

        var lastEnglishBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };

        var lastFirstPriceSealedBid = new DataModelBid
            {
                Bidder = users[1],
                Value = 500,
                Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
            };

        var englishDataModelAuction = new EnglishDataModelAuction
        {
            Description = "English auction",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Seller = users[0],
            StartingPrice = 1000,
            Bids = new List<DataModelBid>
            {
                lastEnglishBid
            }
        };

        var dutchDataModelAuction = new DutchDataModelAuction
        {
            Description = "Dutch auction",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Seller = users[0],
            StartingPrice = 1000,
            PriceDecrementFrequency = new TimeSpan(0, 1, 0),
            PriceDecrementRate = 2
        };

        var firstPriceSealedBidDataModelAuction = new FirstPriceSealedBidDataModelAuction
        {
            Description = "First time sealed bid",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
            Seller = users[2],
            Bids = new List<DataModelBid>
            {
                lastFirstPriceSealedBid
            }
        }; 
        
        List<DataModelAuction> dataAuctions =
        [
            englishDataModelAuction,
            dutchDataModelAuction,
            firstPriceSealedBidDataModelAuction
        ];

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        var bidder = User.Restore(users[0].Id);
        var addedBid = new Bid
        {
            Bidder = bidder,
            Time = new DateTimeOffset(2022, 1, 1, 1, 1, 2, TimeSpan.Zero),
            Value = 10_000
        };
        
        // Act
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            var retrievedAuctions = (await repo.ListAsync()).ToList();
            
            var updatedAuction = (DutchAuction)retrievedAuctions.First(a => a is DutchAuction);
            updatedAuction.Bid = addedBid;
            
            await repo.UpdateAsync(updatedAuction);
            await repo.CommitAsync();
        }
        
        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var updatedBidsDatabaseContent = context.Bids.Include(dataModelBid => dataModelBid.Bidder).ToList();
            var newestDutchBid = updatedBidsDatabaseContent.First(b => b.Value == 10_000);
            
            Assert.Equal(bidder.Id, newestDutchBid.Bidder.Id);
            Assert.Equal(dutchDataModelAuction.Id, newestDutchBid.AuctionId);
            Assert.Equal(addedBid.Value, newestDutchBid.Value);
            
            var updatedAuctionDatabaseContent = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller).ToList();

            Assert.Equal(3, updatedAuctionDatabaseContent.Count);
            
            var dutchDataModelAuctionAfter = updatedAuctionDatabaseContent.First(a => a is DutchDataModelAuction);
            Assert.Equal(dutchDataModelAuction.Id, dutchDataModelAuctionAfter.Id);
            Assert.Equal(dutchDataModelAuction.Description, dutchDataModelAuctionAfter.Description);
            Assert.Equal(dutchDataModelAuction.StartTime, dutchDataModelAuctionAfter.StartTime);
            Assert.Equal(dutchDataModelAuction.EndTime, dutchDataModelAuctionAfter.EndTime);
            Assert.Equal(dutchDataModelAuction.Seller, dutchDataModelAuctionAfter.Seller);
            
            var updatedAuctionBids = dutchDataModelAuctionAfter.Bids.ToList();
            Assert.Single(updatedAuctionBids);
            
            var addedNewAfter = updatedAuctionBids.First();
            
            Assert.Equal(addedBid.Value, addedNewAfter.Value);
            Assert.Equal(addedBid.Time, addedNewAfter.Timestamp);
            Assert.NotEqual(0, dutchDataModelAuctionAfter.Version);
        }
    }
    
    [Fact]
    public async Task Update_FirstPriceSealedBid_Auction_Updated_Bids_Adds_Bid_Commit_Called_Async()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0"
            },
            
            new()
            {
                IdentityId = "1"
            },
            
            new()
            {
                IdentityId = "2"
            }
        };

        var lastEnglishBid = new DataModelBid
        {
            Bidder = users[2],
            Value = 2000,
            Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
        };

        var lastFirstPriceSealedBid = new DataModelBid
            {
                Bidder = users[1],
                Value = 500,
                Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
            };

        var englishDataModelAuction = new EnglishDataModelAuction
        {
            Description = "English auction",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Seller = users[0],
            StartingPrice = 1000,
            Bids = new List<DataModelBid>
            {
                lastEnglishBid
            }
        };

        var dutchDataModelAuction = new DutchDataModelAuction
        {
            Description = "Dutch auction",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            Seller = users[0],
            StartingPrice = 1000,
            PriceDecrementFrequency = new TimeSpan(0, 1, 0),
            PriceDecrementRate = 2
        };

        var firstPriceSealedBidDataModelAuction = new FirstPriceSealedBidDataModelAuction
        {
            Description = "First time sealed bid",
            StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 2, 2, 2, 2, 2, TimeSpan.FromHours(2)),
            Seller = users[2],
            Bids = new List<DataModelBid>
            {
                lastFirstPriceSealedBid
            }
        }; 
        
        List<DataModelAuction> dataAuctions =
        [
            englishDataModelAuction,
            dutchDataModelAuction,
            firstPriceSealedBidDataModelAuction
        ];

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }

        var bidder = User.Restore(users[0].Id);
        var addedBid = new Bid
        {
            Bidder = bidder,
            Time = new DateTimeOffset(2022, 1, 1, 1, 1, 2, TimeSpan.Zero),
            Value = 10_000
        };
        
        // Act
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            var retrievedAuctions = (await repo.ListAsync()).ToList();
            
            var updatedAuction = (FirstPriceSealedBidAuction)retrievedAuctions.First(a => a is FirstPriceSealedBidAuction);
            updatedAuction.Bids.Add(addedBid);
            
            await repo.UpdateAsync(updatedAuction);
            await repo.CommitAsync();
        }
        
        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var updatedBidsDatabaseContent = context.Bids.Include(dataModelBid => dataModelBid.Bidder).ToList();
            var newestFirstPriceSealedBid = updatedBidsDatabaseContent.First(b => b.Value == 10_000);
            
            Assert.Equal(bidder.Id, newestFirstPriceSealedBid.Bidder.Id);
            Assert.Equal(firstPriceSealedBidDataModelAuction.Id, newestFirstPriceSealedBid.AuctionId);
            Assert.Equal(addedBid.Value, newestFirstPriceSealedBid.Value);
            
            var updatedAuctionDatabaseContent = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller).ToList();
            
            var firstPriceSealedBidDataModelAuctionAfter = updatedAuctionDatabaseContent.First(a => a is FirstPriceSealedBidDataModelAuction);
            Assert.Equal(firstPriceSealedBidDataModelAuction.Id, firstPriceSealedBidDataModelAuctionAfter.Id);
            Assert.Equal(firstPriceSealedBidDataModelAuction.Description, firstPriceSealedBidDataModelAuctionAfter.Description);
            Assert.Equal(firstPriceSealedBidDataModelAuction.StartTime, firstPriceSealedBidDataModelAuctionAfter.StartTime);
            Assert.Equal(firstPriceSealedBidDataModelAuction.EndTime, firstPriceSealedBidDataModelAuctionAfter.EndTime);
            Assert.Equal(firstPriceSealedBidDataModelAuction.Seller, firstPriceSealedBidDataModelAuctionAfter.Seller);
            
            var updatedAuctionBids = firstPriceSealedBidDataModelAuctionAfter.Bids.ToList();
            Assert.Equal(2, updatedAuctionBids.Count);
            Assert.Contains(updatedAuctionBids, b => b.Id == lastFirstPriceSealedBid.Id);
            
            var addedNewAfter = updatedAuctionBids.First(b => b.Id != lastFirstPriceSealedBid.Id);
            
            Assert.Equal(addedBid.Value, addedNewAfter.Value);
            Assert.Equal(addedBid.Time, addedNewAfter.Timestamp);
            Assert.NotEqual(0, firstPriceSealedBidDataModelAuctionAfter.Version);
            
        }
    }
    
    [Fact]
    public async Task Update_Set_Endtime_Updates_Nothing_When_Commit_Not_Called_Async()
    {
        // Arrange
        var (users, dataAuctions) = _databaseInitialContent();

        await using (var arrangeContext = new AuctionDbContext(_contextOptions))
        {
            await arrangeContext.AddRangeAsync(users);
            await arrangeContext.AddRangeAsync(dataAuctions);
            await arrangeContext.SaveChangesAsync();
        }
        
        List<DataModelAuction> previousDatabaseContent;
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            previousDatabaseContent = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder)
                .Include(a => a.Seller).ToList();
        }
        
        // Act
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var repo = new AppRepository(context);
            var retrievedAuctions = (await repo.ListAsync()).ToList();
            
            var updatedAuction = retrievedAuctions.First(a => a is FirstPriceSealedBidAuction);
            updatedAuction.EndTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            
            await repo.UpdateAsync(updatedAuction);
        }
        
        // Assert
        await using (var context = new AuctionDbContext(_contextOptions))
        {
            var updatedDatabaseContent = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder).Include(a => a.Seller).ToList();
            Assert.Equal(3, updatedDatabaseContent.Count);
            
            for (int i = 0; i < 3; i++)
            {
                var previousDataAuction = previousDatabaseContent[i];
                var updatedDataAuction = updatedDatabaseContent[i];
                
                AssertDataModelAuctionsMatch(previousDataAuction, updatedDataAuction);
            }
        }
    }
    // TODO: remove the status tests and place them somewhere else (delegate the responsibility to something else)
}