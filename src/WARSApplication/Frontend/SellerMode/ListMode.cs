using Spectre.Console;
using WARSApplication;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.SellerMode;

public class ListMode(ISellMethodsAPI api, IViewController controller) : IView
{
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


        if (infos.Count == 0) return new Result();
        
        var auctionChoice = await AnsiConsole.PromptAsync(
            new SelectionPrompt<SellerAuctionInfo>().AddChoices(infos)
        );
        
        if (statusChoice == AuctionStatus.Finished) return new Result();
        
        var mode = new UpdateMode(auctionChoice, api);
        return await controller.ShowViewAsync(mode);
    }
}