using Frontend.GenericInterfaces;
using Spectre.Console;
using Backend.Data.APIData.Incoming;
using Backend.Data.APIData.Outgoing;

namespace Frontend.BuyerRole;

/// <summary>
/// View for bidding on an auction.
/// </summary>
public class BidViewController(BuyerAuctionInfo info, IBuyMethodsApi api) : IViewController
{
    public string ViewName => "Bid on auction";
    
    /// <summary>
    /// Ask the user for a bid and send it to the API.
    /// </summary>
    public async Task RunAsync()
    {
        AnsiConsole.MarkupLine(info.ToString());
        var bid = await AnsiConsole.AskAsync<decimal?>("Bid: ");

        if (bid is null) return;
        var bidRequest = new BidRequest(info.AuctionId, bid.Value);

        var confirmation = await AnsiConsole.ConfirmAsync($"Are you sure you want to bid {bid.Value} on auction {info.AuctionId}?");
        if (!confirmation) return;
        
        var response = await api.BidAsync(bidRequest);
        AnsiConsole.MarkupLine(response);
    }
}