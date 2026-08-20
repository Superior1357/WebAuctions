using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using WARSApplication;
using WARSApplication.Data.DataModel;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Repository;

namespace UnitTests;

public class AppRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AuctionDbContext> _contextOptions;

    private readonly TimeProvider _fakeTimeProvider = new FakeTimeProvider(
        new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
    );
    
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
    
    // TODO: make these tests
    [Fact]
    public async Task WhereListAsyncMapsDirectlyMappablePropertiesCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
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
            new FirstTimeSealedBidDataModelAuction
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Equal(3, retrievedAuctions.Count);
        for (int i = 0; i < 3; i++)
        {
            var dataAuction = dataAuctions[i];
            var retrievedAuction = retrievedAuctions[i];
            
            Assert.Equal(dataAuction.Id, retrievedAuction.Id);
            Assert.Equal(dataAuction.Description, retrievedAuction.Description);
            Assert.Equal(dataAuction.StartTime, retrievedAuction.StartTime);
            Assert.Equal(dataAuction.EndTime, retrievedAuction.EndTime);
            Assert.Equal(dataAuction.Seller.Id, retrievedAuction.Seller.Id);
            
            switch (dataAuction)
            {
                case EnglishDataModelAuction englishAuction:
                    var englishSpecificData = Assert.IsType<EnglishAuctionRules>(retrievedAuctions[i].Rules);
                    Assert.Equal(englishAuction.StartingPrice, englishSpecificData.StartingPrice);
                    break;
                case DutchDataModelAuction dutchAuction:
                    var dutchSpecificData = Assert.IsType<DutchAuctionRules>(retrievedAuctions[i].Rules);
                    Assert.Equal(dutchAuction.StartingPrice, dutchSpecificData.StartingPrice);
                    Assert.Equal(dutchAuction.PriceDecrementFrequency, dutchSpecificData.PriceDecrementFrequency);
                    Assert.Equal(dutchAuction.PriceDecrementRate, dutchSpecificData.PriceDecrementRate);
                    break;
                case FirstTimeSealedBidDataModelAuction:
                    Assert.IsType<FirstTimeSealedBidRules>(retrievedAuctions[i].Rules);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();    
            }

            if (i != 0)
            {
                Assert.Empty(retrievedAuction.GetBids());
            }
            else
            {
                var bids = dataAuction.Bids.ToList();
                var retrievedBids = retrievedAuction.GetBids().ToList();
                
                Assert.Equal(bids.Count, retrievedBids.Count);

                for (int j = 0; j < bids.Count; j++)
                {
                    var dataBid = bids[0];
                    var retrievedBid = retrievedBids[j];
                    
                    Assert.Equal(dataBid.Bidder.Id, retrievedBid.Bidder.Id);
                    Assert.Equal(dataBid.Value, retrievedBid.Value);
                    Assert.Equal(dataBid.Timestamp, retrievedBid.Time);
                    Assert.Equal(dataBid.Id, retrievedBid.Id);
                }
            }
        }
    }
    
    // TODO: make these tests
    [Fact]
    public async Task WhereListAsyncMapsEnglishActiveStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
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
                    
                    new()
                    {
                    Bidder = users[2],
                    Value = 2000,
                    Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
                    }
                }
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Active, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsEnglishFinishedStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new EnglishDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero),
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
                    
                    new()
                    {
                    Bidder = users[2],
                    Value = 2000,
                    Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
                    }
                }
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Finished, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsEnglishPendingStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new EnglishDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 3, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Pending, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsDutchFinishedAfterBidStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2,
                Bids = new List<DataModelBid>()
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    }
                }
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Finished, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsDutchFinishedAfterTimeEndStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2021, 2, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Finished, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsDutchPendingStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Pending, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsDutchActiveStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new DutchDataModelAuction
            {
                Description = "Dutch auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 2, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                StartingPrice = 1000,
                PriceDecrementFrequency = new TimeSpan(0, 1, 0),
                PriceDecrementRate = 2
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Active, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsFirstTimeSealedBidFinishedStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new FirstTimeSealedBidDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2021, 3, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                Bids = new List<DataModelBid>
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    },
                    
                    new()
                    {
                    Bidder = users[2],
                    Value = 2000,
                    Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
                    }
                }
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Finished, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsFirstTimeSealedBidActiveStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new FirstTimeSealedBidDataModelAuction()
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 3, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0],
                Bids = new List<DataModelBid>
                {
                    new()
                    {
                        Bidder = users[1],
                        Value = 1000,
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero)
                    },
                    
                    new()
                    {
                    Bidder = users[2],
                    Value = 2000,
                    Timestamp = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero)
                    }
                }
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Active, retrievedAuctions[0].Status);   
    }
    
    [Fact]
    public async Task WhereListAsyncMapsFirstTimeSealedBidPendingStatusCorrectly()
    {
        // Arrange
        var users = new List<DataModelUser>
        {
            new()
            {
                IdentityId = "0",
            }, 
            new()
            {
                IdentityId = "1",
            }, 
            new()
            {
                IdentityId = "2"
            }
        };
        
        var dataAuctions = new List<DataModelAuction>
        {
            new FirstTimeSealedBidDataModelAuction
            {
                Description = "English auction",
                StartTime = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2024, 3, 1, 1, 1, 1, TimeSpan.Zero),
                Seller = users[0]
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
            var repo = new AppRepository(context, _fakeTimeProvider);
            retrievedAuctions = (await repo.ListAsync()).ToList();
        }
        
        // Assert
        Assert.Single(retrievedAuctions);
        Assert.Equal(AuctionStatus.Pending, retrievedAuctions[0].Status);   
    }
    
    // TODO: add auction methods (seems simple) - do or not to do any validation? (but do correct auction type mapping tests)
    // TODO: end time parameter adjusting tests - do or not to do any validation?, test one and then multiple updates
    // TODO: test repo commit - if not called, no changes in db, if called, saves changes - what if an exception throw? - capable of handling that?
}