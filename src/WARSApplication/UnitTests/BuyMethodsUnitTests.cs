using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Time.Testing;
using WARSApplication;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
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
                new TestAddOnlyCollection<Bid>([]),
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
                        MakeBid(2, 2000, TestUser, new DateTimeOffset(2021, 3, 1, 1, 1, 2, TimeSpan.Zero))]
                    ),
                1000
            );

            var auction4 = FirstTimeSealedBidAuction.Restore(
                new AuctionId(4),
                "added_before_active_2",
                MakeUser(TestUser.Id.Id + 2),
                new TestAddOnlyCollection<Bid>([MakeBid(3, 1000, TestUser, new DateTimeOffset(2021, 1, 1, 1, 1, 3, TimeSpan.Zero))]),
                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2021, 5, 1, 0, 0, 0, TimeSpan.Zero)
            );

            var auction5 = FirstTimeSealedBidAuction.Restore(
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

    [Fact]
    public async Task List_Sellers_Lists_All_Users_Who_Created_Auctions_Async()
    {
        var addedBeforeContent = MockAuctionsRepoContent;
        var mockRepo = new MockAuctionRepository(addedBeforeContent);

        List<UserId> sellerIds = [addedBeforeContent[0].Seller.Id, addedBeforeContent[1].Seller.Id];

        var result = (await BuyMethods.ListAllSellersAsync(mockRepo)).Value.ToList();

        Assert.Equal(2, result.Count);
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

        var result = (await BuyMethods.ListWonAuctionsAsync(TestUser, mockAuctionRepo)).Value.ToList();

        Assert.Single(result);
        Assert.Contains(addedBeforeAuctionContent[3].Id, result);
    }

    [Fact]
    public async Task List_Non_Finished_Auctions_Lists_All_Non_Finished_Auctions_Of_A_Seller_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var response = await BuyMethods.ListNotFinishedAuctionsOfASellerAsync(addedBeforeAuctionContent[4].Seller.Id, mockAuctionRepo);

        var result = Assert.IsType<Ok<IEnumerable<AuctionId>>>(response.Result).Value.ToList();

        Assert.Single(result);
        Assert.Contains(addedBeforeAuctionContent[5].Id, result);
    }

    [Fact]
    public async Task List_Non_Finished_Auctions_Returns_Error_If_User_ID_Does_Not_Exist_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var response = await BuyMethods.ListNotFinishedAuctionsOfASellerAsync(addedBeforeAuctionContent[4].Seller.Id + 1000, mockAuctionRepo);

        Assert.IsType<NotFound<UserIdNotFound>>(response.Result);
    }

    [Fact]
    public async Task Bid_Bids_To_An_Auction_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var bidRequest = new BidRequest
        {
            Amount = 10_000,
            AuctionId = addedBeforeAuctionContent[5].Id
        };

        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _timeProvider, mockAuctionRepo);

        Assert.IsType<Ok>(response.Result);
        Assert.True(mockAuctionRepo.AllChangesCommited);

        var repoContent = mockAuctionRepo.CommitedRepoContent;
        var correspondingAuction = repoContent.First(a => a.Id == addedBeforeAuctionContent[5].Id);
        Assert.Contains(correspondingAuction.Bids.GetItems(),
            bid => bid.Bidder.Id == TestUser.Id && bid.Value == bidRequest.Amount);
    }

    [Fact]
    public async Task Bid_Returns_Not_Found_When_Auction_Id_Not_Found_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var bidRequest = new BidRequest
        {
            Amount = 10_000,
            AuctionId = addedBeforeAuctionContent[5].Id + 10000
        };

        var response = await BuyMethods.BidAsync(TestUser, bidRequest, _timeProvider, mockAuctionRepo);

        Assert.IsType<NotFound<AuctionIdNotFound>>(response.Result);
    }

    [Fact]
    public async Task Info_Returns_Info_About_Auctions_Async()
    {
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        AuctionId[] requestedIds = [new(5), new(2), new(1)];

        var response = await BuyMethods.GetInfoAsync(requestedIds, mockAuctionRepo);

        var result = Assert.IsType<Ok<IEnumerable<BuyerAuctionInfo>>>(response.Result).Value.ToList();

        Assert.Equal(3, result.Count);

        for (int i = 0; i < 3; i++)
        {
            var info = result[i];
            var auction = addedBeforeAuctionContent.First(a => a.Id == requestedIds[i]);

            Assert.Equal(auction.Id, info.AuctionId);
            Assert.Equal(auction.Seller.Id, info.SellerId);
            Assert.Equal(auction.Description, info.Description);
            Assert.Equal(auction.StartTime, info.StartTime);
            Assert.Equal(auction.EndTime, info.EndTime);
        }

        Assert.IsType<FirstTimeSealedBidInfo>(result[0].InfoData);
        var info2 = Assert.IsType<DutchAuctionInfo>(result[1].InfoData);
        var info1 = Assert.IsType<EnglishAuctionInfo>(result[2].InfoData);

        var rules2 = Assert.IsType<DutchAuction>(addedBeforeAuctionContent[2]);
        var rules1 = Assert.IsType<EnglishAuction>(addedBeforeAuctionContent[1]);

        Assert.Equal(rules2.StartingPrice, info2.CurrentPrice);
        Assert.Equal(rules1.StartingPrice, info1.CurrentBid);
    }

    [Fact]
    public async Task Info_Returns_Info_Returns_Not_Found_When_AuctionId_Nonexistent_Async()
    {
        AuctionId[] requestedIds = [new(5), new(2000), new(1)];
        var addedBeforeAuctionContent = MockAuctionsRepoContent;
        var mockAuctionRepo = new MockAuctionRepository(addedBeforeAuctionContent);

        var response = await BuyMethods.GetInfoAsync(requestedIds, mockAuctionRepo);

        Assert.IsType<NotFound<AuctionIdNotFound>>(response.Result);
    }

    // TODO: auth methods? -- last, after everything else is completed
    // TODO: compare records with normal equality (records are not compare by reference but member-wise)
}
