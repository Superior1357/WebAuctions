using Spectre.Console;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.BuyerMode;

public class BidView(BuyerAuctionInfo info, IBuyMethodsAPI api) : IView
{
    public string Title => "Bid on auction";

    public async Task<IViewResult> RunAsync()
    {
        AnsiConsole.MarkupLine(info.ToString());
        var bid = await AnsiConsole.AskAsync<decimal?>("Bid: ");

        if (bid is null) return new Result();
        var bidRequest = new BidRequest(info.AuctionId, bid.Value);

        var confirmation = await AnsiConsole.ConfirmAsync($"Are you sure you want to bid {bid.Value} on auction {info.AuctionId}?");
        if (!confirmation) return new Result();
        
        var response = await api.BidAsync(bidRequest);
        AnsiConsole.MarkupLine(response);

        return new Result();
    }
}