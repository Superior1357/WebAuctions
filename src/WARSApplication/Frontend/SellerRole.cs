using Frontend.SellerMode;
using Microsoft.Extensions.DependencyInjection;

namespace Frontend;

public class SellerRole(IServiceProvider serviceProvider, IViewController viewController) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        var mode = await PresentModes.PresentAsync(
            serviceProvider.GetRequiredService<CreateMode>(),
            serviceProvider.GetRequiredService<ListMode>()
        );
        
        return await viewController.ShowViewAsync(mode);
    }
}