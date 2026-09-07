using Spectre.Console;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.BuyerMode;

public class ListSellersAuctionsMode(UserId userId, IBuyMethodsAPI api, IViewController viewController) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        var auctions = await api.ListNotFinishedAuctionsOfAUserAsync(userId);
        var infos = await api.ListAuctionsInfoAsync(auctions);
        var info = await AnsiConsole.PromptAsync(
            new SelectionPrompt<BuyerAuctionInfo>()
                .Title("Choose a mode")
                .PageSize(15)
                .UseConverter(mode => mode.ToString())
                .AddChoices(infos)
            );

        var view = new InfoAndBidView(info, api);
        return await viewController.ShowViewAsync(view);
    }
}