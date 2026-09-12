using Spectre.Console;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.BuyerMode;

public class ListAllSellersMode(IBuyMethodsAPI buyMethodsApi, IViewController viewController) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        var users = await buyMethodsApi.ListAllSellersAsync();

        var user = await AnsiConsole.PromptAsync(
            new SelectionPrompt<UserId>()
                .Title("Select a [green]user[/]")
                .PageSize(10)
                .EnableSearch()
                .SearchPlaceholderText("Type to search users...")
                .UseConverter(userId => $"User {userId.Id}")
                .AddChoices(users)
        );

        var mode = new ListSellersAuctionsMode(user, buyMethodsApi, viewController);
        return await viewController.ShowViewAsync(mode);
    }
}