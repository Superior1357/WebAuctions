using Frontend.GenericInterfaces;
using Frontend.Miscellaneous;
using Spectre.Console;
using WARSApplication;

namespace Frontend.Authentication;

/// <summary>
/// Prompt the user for a username and password to register a new account.
/// Save the obtained token in the token bearer.
/// </summary>
public class RegisterMode(IAuthApi authApi, ITokenBearer tokenBearer) : IViewController
{
    public string ViewName => "Register a new user";
    
    /// <summary>
    /// Prompt the user for a username and password to register a new account.
    /// Save the obtained token in the token bearer.
    /// </summary>
    public async Task RunAsync()
    {
        var username = await AnsiConsole.AskAsync<string>("Username: ");
        var password = await AnsiConsole.PromptAsync(new TextPrompt<string>("Password: ").Secret());
        
        var registerRequest = new RegisterRequest(username, password);
        await authApi.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest(username, password);
        
        var token = await authApi.LoginAsync(loginRequest);
        tokenBearer.Token = token.Token;
    }
}