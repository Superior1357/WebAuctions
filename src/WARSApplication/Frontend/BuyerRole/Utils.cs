using Spectre.Console;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.BuyerMode;

public static class Utils
{
    public static Panel BuyerAuctionInfoToListItem(BuyerAuctionInfo buyerInfo)
    {
        var extraContent = buyerInfo switch
        {
            EnglishAuctionInfo englishAuctionInfo => $"""
                                                      Auction type: English
                                                      Current minimum bid: {englishAuctionInfo.CurrentMinimumBid}
                                                      """,
            
            DutchAuctionInfo dutchAuctionInfo => $"""
                                                  Auction type: Dutch
                                                  Current price: {dutchAuctionInfo.CurrentPrice}
                                                  """,
            
            FirstPriceSealedBidInfo => "Auction type: First price sealed bid",
            
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var shownString = $"""
                           Auction id: {buyerInfo.AuctionId.Id}
                           {extraContent}
                           Description: {buyerInfo.Description.Description}
                           Seller id: {buyerInfo.SellerId.Id}
                           Status: {buyerInfo.Status}
                           Start Time: {buyerInfo.StartTime}
                           End Time: {buyerInfo.EndTime}
                           """;

        return new Panel(shownString);
    }
}