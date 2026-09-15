using Spectre.Console;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.BuyerMode;

public class ListSellersAuctionsMode(UserId userId, IBuyMethodsAPI api, IViewController viewController) : IView
{
    private static Panel BuyerAuctionInfoToListItem(BuyerAuctionInfo buyerInfo)
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

    private int currentIndex = 0;
    public string Title => "List seller's auctions";
    public async Task<IViewResult> RunAsync()
    {
        var auctions = await api.ListNotFinishedAuctionsOfAUserAsync(userId);
        var infos = (await api.ListAuctionsInfoAsync(auctions)).ToList();

        var scrollingListView = new ScrollingListView<BuyerAuctionInfo>("Choose an auction to bid", infos, BuyerAuctionInfoToListItem);
        var choice = await scrollingListView.MakeChoiceAsync();

        if (choice == null)
            return new Result();
        
        var view = new BidView(choice, api);
        return await viewController.ShowViewAsync(view);
    }
}