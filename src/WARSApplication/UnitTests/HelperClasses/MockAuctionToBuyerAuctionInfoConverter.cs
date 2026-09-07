using WARSApplication.Data;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace UnitTests.HelperClasses;

internal class MockAuctionToSellerAuctionInfoConverter(Dictionary<AuctionId, SellerAuctionInfo> mapping) : IDataConverter<Auction, SellerAuctionInfo>
{
    public SellerAuctionInfo Convert(Auction entity) => mapping[entity.Id];
}