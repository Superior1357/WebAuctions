using Frontend.GenericInterfaces;
using Frontend.SellerRole;
using Frontend.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Frontend.MainMenu;

/// <summary>
/// Present the user with the options to create an auction or list all auctions.
/// </summary>
public class SellerRole(IServiceProvider serviceProvider, IMasterController masterController) : IViewController
{
    public string ViewName => "Seller role";

    /// <summary>
    /// Present the user with the options to create an auction or list all auctions.
    /// </summary>
    public async Task RunAsync()
    {
        var mode = await PresentOptions.PresentAsync(
            serviceProvider.GetRequiredService<CreateMode>(),
            serviceProvider.GetRequiredService<ListMode>()
        );
        
        await masterController.ShowViewAsync(mode);
    }
}