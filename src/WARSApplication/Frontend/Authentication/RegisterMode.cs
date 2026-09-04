using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using WARSApplication;

namespace Frontend.Authentication;

public class RegisterMode(IAuthApi authApi, IServiceProvider serviceProvider) : IView
{
    public async Task<IViewResult> RunAsync()
    {
        var username = await AnsiConsole.AskAsync<string>("Username: ");
        var password = await AnsiConsole.PromptAsync(new TextPrompt<string>("Password: ").Secret());
        
        var registerRequest = new RegisterRequest(username, password);
        await authApi.RegisterAsync(registerRequest);

        _ = await serviceProvider.GetRequiredService<LoginMode>().RunAsync();
        return new Result();
    }
}