using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using WARSApplication;
using WARSApplication.Data.DataModel;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Repository;

namespace UnitTests;
// TODO: throw exception when StartTime is after EndTime (not supporting that, don't test any other possible problems)
// TODO: add auction methods (seems simple) - do correct auction type mapping tests
// TODO: end time parameter adjusting tests - test one and then multiple updates
// TODO: test repo commit - if not called, no changes in db, if called, saves changes - what if an exception throw? - capable of handling that?
[TestSubject(typeof(AppRepository))]
public class AppRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AuctionDbContext> _contextOptions;
    private readonly TimeProvider _fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero));

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
                },
                LastPlacedBid = lastBid.Id
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
                break;
            case DutchDataModelAuction dutchAuction:
                var dutchSpecificData = Assert.IsType<DutchAuction>(businessAuction);
                Assert.Equal(dutchAuction.StartingPrice, dutchSpecificData.StartingPrice);
                Assert.Equal(dutchAuction.PriceDecrementFrequency, dutchSpecificData.PriceDecrementFrequency);
                Assert.Equal(dutchAuction.PriceDecrementRate, dutchSpecificData.PriceDecrementRate);
                break;
            case FirstPriceSealedBidDataModelAuction:
                Assert.IsType<FirstPriceSealedBidAuction>(businessAuction);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        var bids = dataAuction.Bids.ToList();
        var retrievedBids = businessAuction.Bids.GetItems().ToList();
            
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

    private static void AssertDataModelAuctionsMatch(DataModelAuction expected, DataModelAuction actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.StartTime, actual.StartTime);
        Assert.Equal(expected.EndTime, actual.EndTime);
        Assert.Equal(expected.Seller, actual.Seller);
        Assert.Equal(expected.LastPlacedBid, actual.LastPlacedBid);
        
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
                },
                LastPlacedBid = lastBid.Id
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
                EndTime = new DateTimeOffset(2025, 2,2,2,2,2,TimeSpan.FromHours(2)),
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
                },
                LastPlacedBid = lastBid.Id
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
                EndTime = new DateTimeOffset(2025, 2,2,2,2,2,TimeSpan.FromHours(2)),
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
            var auctions = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder).Include(a => a.Seller).ToList();
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
                },
                LastPlacedBid = lastBid.Id
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
                EndTime = new DateTimeOffset(2025, 2,2,2,2,2,TimeSpan.FromHours(2)),
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
            var auctions = context.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder).Include(a => a.Seller).ToList();
            dataAuctions = dataAuctions.OrderBy(dataAuction => dataAuction.Id.Id).ToList();
            Assert.Equal(3, auctions.Count);
            for (int i = 0; i < 3; i++)
                AssertDataModelAuctionsMatch(dataAuctions[i], auctions[i]);
        }
    } 
    
    [Fact]
    public async Task UpdateAsyncUpdatesAuctionWhenCommitCalled()
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
            
            Assert.Contains(retrievedAuctions, a => a is FirstPriceSealedBidAuction);
            var updatedAuction = retrievedAuctions.First(a => a is FirstPriceSealedBidAuction);
            updatedAuction.EndTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            
            await repo.UpdateAsync(updatedAuction);
            await repo.CommitAsync();
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

                if (i == 2)
                {
                    var firstTimeSealedBidDataAuction = Assert.IsType<FirstPriceSealedBidDataModelAuction>(updatedDataAuction);
                    
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
    public async Task UpdateAsyncUpdatesNothingWhenCommitNotCalled()
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
    
    
    // TODO: combined tests (e.g. add + update) -> if makes sense to combine them? (but do not overthink this)
    // TODO: remove the status tests and place them somewhere else (delegate the responsibility to something else)
}


