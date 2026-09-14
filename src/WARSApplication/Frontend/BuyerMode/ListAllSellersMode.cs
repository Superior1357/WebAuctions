using Spectre.Console;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.BuyerMode;

public class ListAllSellersMode(IBuyMethodsAPI buyMethodsApi, IViewController viewController) : IView
{
    private record UserIdClass(UserId Id);
    private static Panel IdToPanel(UserIdClass id) => new ($"User ID: {id.Id.Id}");
    
    public async Task<IViewResult> RunAsync()
    {
        var users = (await buyMethodsApi.ListAllSellersAsync()).Select(user => new UserIdClass(user)).ToList();

        var view = new ScrollingListView<UserIdClass>(users, IdToPanel);
        var user = await view.MakeChoiceAsync();
        
        if (user is null) return new Result();
        
        var mode = new ListSellersAuctionsMode(user.Id, buyMethodsApi, viewController);
        return await viewController.ShowViewAsync(mode);
    }
}