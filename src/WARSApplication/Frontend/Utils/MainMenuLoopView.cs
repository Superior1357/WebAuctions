using Frontend.GenericInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Frontend.Utils;

/// <summary>
/// Present the user with the options to buy or sell.
/// </summary>
public class MainMenuLoopViewController(IServiceProvider services, IMasterController masterController) : IViewController
{
    public string ViewName => "Main menu";
    
    /// <summary>
    /// Present the user with the options to buy or sell.
    /// Keep presenting the user with the options until the user quits.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            IViewController[] menuOptions =
            [
                services.GetRequiredService<MainMenu.BuyerRole>(),
                services.GetRequiredService<MainMenu.SellerRole>()
            ];
            
            var mode = await PresentOptions.PresentAsync(menuOptions);
            await masterController.ShowViewAsync(mode);
            Utils.MakeConfirmationPrompt();
        }
        // ReSharper disable once FunctionNeverReturns
    }
}