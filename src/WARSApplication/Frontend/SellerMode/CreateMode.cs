using Spectre.Console;
using WARSApplication.Data.APIData.Incoming;

namespace Frontend.SellerMode;

public class CreateMode(ISellMethodsAPI api) : IView
{
    private enum AuctionType
    {
        English,
        Dutch,
        FirstTimeSealedBid
    }
    
    public async Task<IViewResult> RunAsync()
    {
        var description = await AnsiConsole.AskAsync<string>("Description: ");

        var startTime = await Utils.PromptTimeAsync("Start time: ");
        var endTime = await Utils.PromptTimeAsync("End time: ");
        
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
            case "FirstTimeSealedBid":
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
        return new Result();
    }
}