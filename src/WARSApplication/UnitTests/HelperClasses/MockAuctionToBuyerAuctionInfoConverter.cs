using WARSApplication.Data;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace UnitTests.HelperClasses;

public class MockAuctionToSellerAuctionInfoConverter(Dictionary<AuctionId, SellerAuctionInfo> mapping) : IDataConverter<Auction, SellerAuctionInfo>
{
    public SellerAuctionInfo Convert(Auction item) => mapping[item.Id];
}