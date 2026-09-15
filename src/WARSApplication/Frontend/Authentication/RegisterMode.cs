using Spectre.Console;
using WARSApplication;

namespace Frontend.Authentication;

public class RegisterMode(IAuthApi authApi, ITokenBearer tokenBearer) : IView
{
    public string Title => "Register a new user";

    public async Task<IViewResult> RunAsync()
    {
        var username = await AnsiConsole.AskAsync<string>("Username: ");
        var password = await AnsiConsole.PromptAsync(new TextPrompt<string>("Password: ").Secret());
        
        var registerRequest = new RegisterRequest(username, password);
        await authApi.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest(username, password);
        
        var token = await authApi.LoginAsync(loginRequest);
        tokenBearer.Token = token.Token;
        
        return new Result();
    }
}