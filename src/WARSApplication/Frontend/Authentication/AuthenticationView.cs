using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Frontend.Authentication;

public class AuthenticationView(ITokenBearer tokenBearer, IViewController viewController, IServiceProvider services) : IView
{
    public string Title => "Authentication";
    public async Task<IViewResult> RunAsync()
    {
        do
        {
            AnsiConsole.Clear();
            
            var authMode = await PresentOptions.PresentAsync(
                services.GetRequiredService<LoginMode>(),
                services.GetRequiredService<RegisterMode>()
            );

            await viewController.ShowViewAsync(authMode);
            Utils.MakeConfirmationPrompt();
        } while (tokenBearer.Token == null);
        
        var authApi = services.GetRequiredService<IIdentityApi>();
        var userId = await authApi.RevealUserId();
        
        AnsiConsole.WriteLine($"Your user id is {userId.Id}");
        Utils.MakeConfirmationPrompt();

        return new Result();
    }
}