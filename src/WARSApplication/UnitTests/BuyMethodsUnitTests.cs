using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Time.Testing;
using WARSApplication;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using BuyMethods = WARSApplication.BusinessMethods.BuyMethods;

namespace UnitTests;

public class BuyMethodsUnitTests
{
    private readonly TimeProvider _timeProvider = new FakeTimeProvider(
        new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
    );

    private static User TestUser { get; } = new()
    {
        Id = new UserId(2),
        IdentityId = "123456789"
    };
    
    private static Auction[] MockAuctionsRepoContent => 
    [
        new Auction
        {
            Id = new AuctionId(0),
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

        new()
        {
            Id = new AuctionId(1),
            Seller = TestUser,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before3",
            Type = AuctionType.English,
            InitialPrice = 1000
        },
        
        new()
        {
            Id = new AuctionId(2),
            Seller = TestUser,
            StartTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before2",
            Type = AuctionType.Dutch,
            InitialPrice = 1000
        },
        
        new()
        {
            Id = new AuctionId(3),
            Seller = new User
            {
                Id = TestUser.Id + 1,
                IdentityId = "AnotherUser"
            },
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before_active_1",
            Type = AuctionType.English,
            InitialPrice = 1000,
            Status = AuctionStatus.Active
        },
        
        new()
        {
        Id = new AuctionId(4),
        Seller = new User
        {
            Id = TestUser.Id + 2,
            IdentityId = "AnotherUser"
        },
        StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
        EndTime = new DateTimeOffset(2021, 5, 1, 0, 0, 0, TimeSpan.Zero),
        Description = "added_before_active_2",
        Type = AuctionType.FirstTimeSealedBid,
        Status = AuctionStatus.Finished
        },
        
        new()
        {
            Id = new AuctionId(5),
            Seller = new User
            {
                Id = TestUser.Id + 2,
                IdentityId = "AnotherUser"
            },
            StartTime = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "added_before_active_3",
            Type = AuctionType.FirstTimeSealedBid,
            Status = AuctionStatus.Active
        }
    ];

    private static Bid[] MockBidRepoContent()
    {
        var auctionContentCopy = MockAuctionsRepoContent;
        
        return [
            new Bid
            {
                Id = 0,
                Amount = 1000,
                Bidder = TestUser,
                Time = new DateTimeOffset(2021, 1, 1, 1, 1, 2, TimeSpan.Zero),
                Auction = auctionContentCopy[3]
            },
            
            new Bid
            {
                Id = 1,
                Amount = 1500,
                Bidder = new User
                {
                    Id = TestUser.Id + 2,
                    IdentityId = "AnotherUser"
                },
                Time = new DateTimeOffset(2021, 2, 1, 1, 1, 2, TimeSpan.Zero),
                Auction = auctionContentCopy[3]
            },
            
            new Bid
            {
                Id = 2,
                Amount = 2000,
                Bidder = TestUser,
                Time = new DateTimeOffset(2021, 3, 1, 1, 1, 2, TimeSpan.Zero),
                Auction = auctionContentCopy[3]
            },
            
            new Bid
            {
                Id = 3,
                Amount = 1000,
                Bidder = TestUser,
                Time = new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero),
                Auction = auctionContentCopy[4]
            }
        ];
    }
    
    [Fact]
    public async Task List_Sellers_Lists_All_Users_Who_Created_Auctions_Async()
    {
        // Arrange
        var addedBeforeContent = MockAuctionsRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);
    
        List<UserId> sellerIds = [addedBeforeContent[0].Seller.Id, addedBeforeContent[1].Seller.Id];
        
        // Act
        var result = (await BuyMethods.ListAllSellersAsync(mockRepo)).Value.ToList();
        
        // Assert
        Assert.Equal(2, result.Count);
        foreach (var id in sellerIds)
        {
            Assert.Contains(id, result);
        }
    }
    
    [Fact]
    public async Task List_Participated_Lists_All_Auctions_That_User_Participated_In_Async()
    {
        // Arrange
        var addedBeforeBidContent = MockBidRepoContent();
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        
        var mockBidRepo = new MockBidRepository(addedBeforeBidContent);
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);
        
        // Act
        var result = (await BuyMethods.ListParticipatedAuctionsAsync(TestUser, mockBidRepo, mockAuctionRepo)).Value.ToList();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(addedBeforeBidContent[3].Auction.Id, result);
        Assert.Contains(addedBeforeBidContent[4].Auction.Id, result);
    }
    
    [Fact]
    public async Task List_Won_Lists_All_Auctions_That_User_Won_Async()
    {
        // Arrange
        var addedBeforeBidContent = MockBidRepoContent();
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        
        var mockBidRepo = new MockBidRepository(addedBeforeBidContent);
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);
        
        // Act
        var result = (await BuyMethods.ListWonAuctionsAsync(TestUser, mockBidRepo, mockAuctionRepo)).Value.ToList();
        
        // Assert
        Assert.Single(result);
        Assert.Contains(addedBeforeBidContent[3].Auction.Id, result);
    }
    
    [Fact]
    public async Task List_Non_Finished_Auctions_Lists_All_Non_Finished_Auctions_Of_A_Seller_Async()
    {
        // Arrange
        var addedBeforeBidContent = MockBidRepoContent();
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        
        var mockBidRepo = new MockBidRepository(addedBeforeBidContent);
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);
        
        // Act
        var response = await BuyMethods.ListNotFinishedAuctionsOfASellerAsync(addedBeforeAuctionContent[4].Seller.Id, mockAuctionRepo);
        
        // Assert
        var result = Assert.IsType<Ok<IEnumerable<AuctionId>>>(response.Result).Value.ToList();
                
        Assert.Single(result);
        Assert.Contains(addedBeforeBidContent[5].Auction.Id, result);
    }
    
    [Fact]
    public async Task List_Non_Finished_Auctions_Returns_Error_If_User_ID_Does_Not_Exist_Async()
    {
        // Arrange
        var addedBeforeBidContent = MockBidRepoContent();
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        
        var mockBidRepo = new MockBidRepository(addedBeforeBidContent);
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);
        
        // Act
        var response = await BuyMethods.ListNotFinishedAuctionsOfASellerAsync(addedBeforeAuctionContent[4].Seller.Id + 1000, mockAuctionRepo);
        
        // Assert
        Assert.IsType<NotFound<UserIdNotFound>>(response.Result);
    }
    
    
    // TODO: complete buy methods
    // TODO: auth methods? -- last, after everything else is completed
}