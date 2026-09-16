using Frontend.BuyerMode;
using Microsoft.Extensions.DependencyInjection;

namespace Frontend;

public class BuyerRole(IViewController controller, IServiceProvider serviceProvider) : IView
{
    public string Title => "Buyer role";

    public async Task<IViewResult> RunAsync()
    {
        var view = await PresentOptions.PresentAsync(
            serviceProvider.GetRequiredService<ListAllSellersMode>(),
                        serviceProvider.GetRequiredService<ListParticipatedAuctionsView>(),
                        serviceProvider.GetRequiredService<ListWonAuctionsView>()
            );
        
        return await controller.ShowViewAsync(view);
    }
}
