using Frontend.GenericInterfaces;
using Frontend.Miscellaneous;
using Spectre.Console;
using WARSApplication;

namespace Frontend.Authentication;
/// <summary>
/// Prompt the user for a username and password to log in to a registered account.
/// Save the obtained token in the token bearer.
/// </summary>
public class LoginMode(IAuthApi authApi, ITokenBearer tokenBearer) : IViewController
{
    public string ViewName => "Login";
    /// <summary>
    /// Prompt the user for a username and password to log in to a registered account.
    /// </summary>
    public async Task RunAsync()
    {
        var username = await AnsiConsole.AskAsync<string>("Username: ");
        var password = await AnsiConsole.PromptAsync(new TextPrompt<string>("Password: ").Secret());

        var loginRequest = new LoginRequest(username, password);
        var token = await authApi.LoginAsync(loginRequest);

        tokenBearer.Token = token.Token;
    }
}