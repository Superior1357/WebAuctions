using Spectre.Console;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.BuyerRole;

public static class Utils
{
    /// <summary>
    /// Converts a BuyerAuctionInfo to a Panel that can be shown in the list view.
    /// </summary>
    /// <param name="buyerInfo">the shown buyer info</param>
    /// <returns>a panel that represents the shown buyer info</returns>
    /// <exception cref="ArgumentOutOfRangeException">when a new auction type has been added and the switch expression not altered</exception>
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