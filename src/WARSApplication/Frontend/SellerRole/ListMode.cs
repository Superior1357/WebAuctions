using Frontend.GenericInterfaces;
using Frontend.Utils;
using Spectre.Console;
using Backend;
using Backend.Data.APIData.Outgoing;
namespace Frontend.SellerRole;

/// <summary>
/// View for listing all auctions the user instantiated.
/// </summary>
/// <param name="api"></param>
/// <param name="controller"></param>
public class ListMode(ISellMethodsApi api, IMasterController controller) : IViewController
{
    private static Panel SellerAuctionInfoToPanel(SellerAuctionInfo info)
    {
        var auctionTypeSpecificContent = info switch
        {
            EnglishSellerAuctionInfo englishAuctionInfo => $"""
                                                            Auction type: English
                                                            Starting price: {englishAuctionInfo.StartingPrice.Value}
                                                            """,
            
            DutchSellerAuctionInfo dutchAuctionInfo => $"""
                                                  Auction type: Dutch
                                                  Starting price: {dutchAuctionInfo.StartingPrice.Value}
                                                  Price decrement rate: {dutchAuctionInfo.PriceDecrementRate.Value}
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
                   Winning bidder Id: {finishedStatus.Winner.Value.WinningBidderId.Id}
                   Winning bid: {finishedStatus.Winner.Value.WinningBid.Value}
                   """
                : "Auction status: Finished",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var shownString = $"""
                           Auction id: {info.Id.Id}
                           {auctionTypeSpecificContent}
                           Description: {info.Description.Description}
                           Start Time: {info.StartTime}
                           End Time: {info.EndTime}
                           {statusSpecificContent}
                           """;

        return new Panel(shownString);
    }

    public string ViewName => "List user's auctions";

    /// <summary>
    /// Ask the user to choose an auction he created and show the update view.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task RunAsync()
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
                    })
                    .AddChoices(AuctionStatus.Pending, AuctionStatus.Active, AuctionStatus.Finished)
            );

        var auctions = await api.ListAuctionsAsync(statusChoice);
        var infos = (await api.GetInfoAsync(auctions)).ToList();

        var list = new ScrollingListViewController<SellerAuctionInfo>("Choose an auction to update", infos, SellerAuctionInfoToPanel);
        var choice = await list.MakeChoiceAsync();
        
        if (choice is null || statusChoice == AuctionStatus.Finished)
            return;

        var mode = new UpdateMode(choice, api);
        await controller.ShowViewAsync(mode);
    }
}