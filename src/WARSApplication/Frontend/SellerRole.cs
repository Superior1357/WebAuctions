using Frontend.SellerMode;
using Microsoft.Extensions.DependencyInjection;

namespace Frontend;

public class SellerRole(IServiceProvider serviceProvider) : IProgramMode
{
    public async Task<IModeResult> RunAsync()
    {
        var mode = await PresentModes.PresentAsync([
            serviceProvider.GetRequiredService<CreateMode>(),
            serviceProvider.GetRequiredService<ListMode>(),
            serviceProvider.GetRequiredService<UpdateMode>()
        ]);
        
        await mode.RunAsync();
        return new Result();
    }
}