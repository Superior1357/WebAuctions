using Frontend.GenericInterfaces;
using Spectre.Console;
using Backend.Data.APIData.Incoming;

namespace Frontend.SellerRole;

/// <summary>
/// Control the creation of an auction.
/// </summary>
/// <param name="api"></param>
public class CreateMode(ISellMethodsApi api) : IViewController
{
    private enum AuctionType
    {
        English,
        Dutch,
        FirstPriceSealedBid
    }
    public string ViewName => "Create auction";
    
    /// <summary>
    /// Prompt the user for the auction's parameters and send them to the API.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task RunAsync()
    {
        var description = await AnsiConsole.AskAsync<string>("Description: ");

        var startTime = await Utils.Utils.PromptTimeAsync("Start time: ");
        var endTime = await Utils.Utils.PromptTimeAsync("End time: ");
        
        var type = await AnsiConsole.PromptAsync(
            new SelectionPrompt<string>()
                .Title("Choose auction type")
                .AddChoices(Enum.GetNames<AuctionType>())
            );

        CreatedAuction createdAuction;
        
        switch (type)
        {
            case "English":
                createdAuction = new EnglishCreatedAuction
                {
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime,
                    StartingPrice = await AnsiConsole.AskAsync<decimal>("Starting price: ")
                };
                
                break;
            case "Dutch":
                var decrementFrequency = await AnsiConsole.PromptAsync(
                    new TextPrompt<TimeSpan>("Choose the decrement frequency: ")
                        .DefaultValue(new TimeSpan(1,0,0,0))
                        .EditableDefaultValue(true)
                );

                createdAuction = new DutchCreatedAuction
                {
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime,
                    PriceDecrementFrequency = decrementFrequency,
                    StartingPrice = await AnsiConsole.AskAsync<decimal>("Starting price: "),
                    PriceDecrementRate = await AnsiConsole.AskAsync<decimal>("Price decrement rate: ")
                };
                
                break;
            case "FirstPriceSealedBid":
                createdAuction = new FirstPriceSealedBidCreatedAuction
                {
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime
                };
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await api.CreateAuctionAsync(createdAuction);
    }
}