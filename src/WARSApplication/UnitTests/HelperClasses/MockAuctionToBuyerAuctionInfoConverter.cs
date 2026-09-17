using Backend.Data;
using Backend.Data.APIData.Outgoing;
using Backend.Data.BusinessModel.Business;
using Backend.Data.SharedDataProperties;

namespace UnitTests.HelperClasses;

internal class MockAuctionToSellerAuctionInfoConverter(Dictionary<AuctionId, SellerAuctionInfo> mapping) : IDataConverter<Auction, SellerAuctionInfo>
{
    public SellerAuctionInfo Convert(Auction entity) => mapping[entity.Id];
}