using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using WARSApplication;

namespace Frontend.Authentication;

public class RegisterMode(IAuthApi authApi, IViewController controller) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        var username = await AnsiConsole.AskAsync<string>("Username: ");
        var password = await AnsiConsole.PromptAsync(new TextPrompt<string>("Password: ").Secret());
        
        var registerRequest = new RegisterRequest(username, password);
        await authApi.RegisterAsync(registerRequest);

        return await controller.ShowViewAsync<LoginMode>();
    }
}