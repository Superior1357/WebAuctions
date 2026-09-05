using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.Data.DataConverters;

public class AuctionToSellerAuctionInfo(IAuctionEvaluator auctionEvaluator) : IDataConverter<Auction, SellerAuctionInfo>
{
    public SellerAuctionInfo Convert(Auction item)
    {
        throw new NotImplementedException();
    }
}