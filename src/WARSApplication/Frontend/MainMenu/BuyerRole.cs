using Frontend.BuyerRole;
using Frontend.GenericInterfaces;
using Frontend.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Frontend.MainMenu;

/// <summary>
/// Present the user with the options to buy an auction or list all auctions.
/// </summary>
public class BuyerRole(IMasterController controller, IServiceProvider serviceProvider) : IViewController
{
    public string ViewName => "Buyer role";

    /// <summary>
    /// Present the user with the options to buy an auction or list all auctions.
    /// </summary>
    public async Task RunAsync()
    {
        var view = await PresentOptions.PresentAsync(
            serviceProvider.GetRequiredService<ListAllSellersMode>(),
                        serviceProvider.GetRequiredService<ListParticipatedAuctionsViewController>(),
                        serviceProvider.GetRequiredService<ListWonAuctionsViewController>()
            );
        
        await controller.ShowViewAsync(view);
    }
}
