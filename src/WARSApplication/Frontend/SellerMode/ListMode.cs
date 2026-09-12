using Spectre.Console;
using WARSApplication;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.SellerMode;

public class ListMode(ISellMethodsAPI api, IViewController controller) : IView
{
    private static Panel SellerAuctionInfoToPanel(SellerAuctionInfo info)
    {
        var auctionTypeSpecificContent = info switch
        {
            EnglishSellerAuctionInfo englishAuctionInfo => $"""
                                                      Auction type: English
                                                      Starting price: {englishAuctionInfo.StartingPrice}
                                                      """,
            
            DutchSellerAuctionInfo dutchAuctionInfo => $"""
                                                  Auction type: Dutch
                                                  Starting price: {dutchAuctionInfo.StartingPrice}
                                                  Price decrement rate: {dutchAuctionInfo.PriceDecrementRate}
                                                  Price decrement frequency: {dutchAuctionInfo.PriceDecrementFrequency}
                                                  """,
            
            FirstPriceSealedBidSellerAuctionInfo => "Auction type: First price sealed bid",
                        
            _ => throw new ArgumentOutOfRangeException()
        };

        var statusSpecificContent = info.Info switch
        {
            PendingStatus => "Auction status: Pending",
            ActiveStatus => "Auction status: Active",
            FinishedStatus finishedStatus => finishedStatus.Winner.HasValue
                ? $"""
                   Auction status: Finished
                   Winning bidder Id: {finishedStatus.Winner.Value.WinningBidderId}
                   Winning bid: {finishedStatus.Winner.Value.WinningBid.Value}
                   """
                : "Auction status: Finished",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var shownString = $"""
                           Auction id: {info.Id}
                           {auctionTypeSpecificContent}
                           Description: {info.Description}
                           Start Time: {info.StartTime}
                           End Time: {info.EndTime}
                           {statusSpecificContent}
                           """;

        return new Panel(shownString);
    }
    
    public async Task<IViewResult> RunAsync()
    {
        var statusChoice = await AnsiConsole.PromptAsync(
                new SelectionPrompt<AuctionStatus>()
                    .UseConverter(status =>
                    {
                        return status switch
                        {
                            AuctionStatus.Pending => "Pending",
                            AuctionStatus.Active => "Active",
                            AuctionStatus.Finished => "Finished",
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }).AddChoices(AuctionStatus.Pending, AuctionStatus.Active, AuctionStatus.Finished)
            );

        var auctions = await api.ListAuctionsAsync(statusChoice);
        var infos = (await api.GetInfoAsync(auctions)).ToList();

        var list = new ScrollingListView<SellerAuctionInfo>(infos, SellerAuctionInfoToPanel);
        var choice = await list.MakeChoiceAsync();
        
        if (choice is null || statusChoice == AuctionStatus.Finished)
            return new Result();

        var mode = new UpdateMode(choice, api);
        return await controller.ShowViewAsync(mode);
    }
}