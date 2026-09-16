using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Frontend;

public class MainMenuLoopView(IServiceProvider services, IViewController viewController) : IView
{
    public string Title => "Main menu";
    public async Task<IViewResult> RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();

            IView[] menuOptions =
            [
                services.GetRequiredService<BuyerRole>(),
                services.GetRequiredService<SellerRole>()
            ];
            
            var mode = await PresentOptions.PresentAsync(menuOptions);
            _ = await viewController.ShowViewAsync(mode);
            
            Utils.MakeConfirmationPrompt();
        }
    }
}