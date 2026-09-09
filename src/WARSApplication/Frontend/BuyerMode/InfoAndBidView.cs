using Spectre.Console;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.BuyerMode;

public class InfoAndBidView(BuyerAuctionInfo info, IBuyMethodsAPI api) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        AnsiConsole.MarkupLine(info.ToString());
        var bid = await AnsiConsole.AskAsync<decimal?>("Bid: ");
        
        if (bid is not null)
        {
            var bidRequest = new BidRequest(info.AuctionId, bid.Value);

            var confirmation = await AnsiConsole.ConfirmAsync($"Are you sure you want to bid {bid.Value} on auction {info.AuctionId}?");
            if (confirmation)
            {
                var response = await api.BidAsync(bidRequest);
                AnsiConsole.MarkupLine(response);
            }
        }

        await AnsiConsole.PromptAsync(new ConfirmationPrompt("Click to continue"));
        return new Result();
    }
}