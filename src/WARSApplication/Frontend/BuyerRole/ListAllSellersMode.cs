using Frontend.GenericInterfaces;
using Frontend.Utils;
using Spectre.Console;
using Backend.Data.SharedDataProperties;

namespace Frontend.BuyerRole;

/// <summary>
/// View controller for listing all sellers.
/// </summary>
public class ListAllSellersMode(IBuyMethodsApi buyMethodsApi, IMasterController masterController) : IViewController
{
    private record UserIdClass(UserId Id);
    private static Panel IdToPanel(UserIdClass id) => new ($"User ID: {id.Id.Id}");
    public string ViewName => "List all sellers";
    
    /// <summary>
    /// Present the user a list of ids of all sellers.
    /// Ask the user to choose one and show all auctions of that seller.
    /// </summary>
    public async Task RunAsync()
    {
        var users = (await buyMethodsApi.ListAllSellersAsync()).Select(user => new UserIdClass(user)).ToList();

        var view = new ScrollingListViewController<UserIdClass>("Choose a seller to list all his auctions.", users, IdToPanel);
        var user = await view.MakeChoiceAsync();
        
        if (user is null) return;
        
        var mode = new ListSellersAuctionsViewController(user.Id, buyMethodsApi, masterController);
        await masterController.ShowViewAsync(mode);
    }
}