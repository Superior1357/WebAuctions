using Frontend.BuyerMode;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Frontend;

public class BuyerRole(IServiceProvider serviceProvider) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        var mode = await PresentModes.PresentAsync(
            serviceProvider.GetRequiredService<ListAllSellersMode>()
            );
        
        await mode.RunAsync();
        return new Result();
    }
}