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
        // TODO: time
        
        var type = await AnsiConsole.PromptAsync(
            new SelectionPrompt<string>().Title("Choose auction type").AddChoices(Enum.GetNames<AuctionType>())
            );

        CreatedAuction createdAuction;
        
        switch (type)
        {
            case "English":
                break;
            case "Dutch":
                break;
            case "FirstTimeSealedBid":
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        throw new NotImplementedException();
        
        return new Result();
    }
}