using Frontend.GenericInterfaces;
using Frontend.Miscellaneous;
using Frontend.Utils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Frontend.Authentication;
/// <summary>
/// Give the user the option to register or login.
/// </summary>
public class AuthenticationViewController(ITokenBearer tokenBearer, IMasterController masterController, IServiceProvider services) : IViewController
{
    public string ViewName => "Authentication";
    
    /// <summary>
    /// Present the user the option to register or login.
    /// </summary>
    public async Task RunAsync()
    {
        do
        {
            AnsiConsole.Clear();
            
            var authMode = await PresentOptions.PresentAsync(
                services.GetRequiredService<LoginMode>(),
                services.GetRequiredService<RegisterMode>()
            );

            await masterController.ShowViewAsync(authMode);
            Utils.Utils.MakeConfirmationPrompt();
        } while (tokenBearer.Token == null);
        
        var authApi = services.GetRequiredService<IIdentityApi>();
        var userId = await authApi.RevealUserId();
        
        AnsiConsole.WriteLine($"Your user id is {userId.Id}");
        Utils.Utils.MakeConfirmationPrompt();
    }
}