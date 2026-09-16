using Frontend.GenericInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Spectre.Console;

namespace Frontend.Utils;

/// <summary>
/// The master controller that safely controls individual view executions.
/// It handles exceptions and shows error messages to the user.
/// </summary>
/// <param name="serviceProvider"></param>
public class MasterController(IServiceProvider serviceProvider) : IMasterController
{
    public Task ShowViewAsync<T>() where T : IViewController
    {
        var view = serviceProvider.GetRequiredService<T>();
        return ShowViewAsync(view);
    }

    /// <summary>
    /// Shows a view and handles exceptions.
    /// </summary>
    /// <param name="viewController"></param>
    public async Task ShowViewAsync(IViewController viewController)
    {
        try
        {
            await viewController.RunAsync();
        }
        
        catch (ApiException e)
        {
            await ShowApiErrorAsync(e);
        }
        
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"The following error: {e.Message} occured.");
        }
    }

    private static async Task ShowApiErrorAsync(ApiException e)
        => AnsiConsole.MarkupLine($"The following error: {await ExtractMessageAsync(e)} occured.");

    private static async Task<string> ExtractMessageAsync(ApiException e)
    {
        try
        {
            var error = await e.GetContentAsAsync<ApiErrorBody>();
            var errorBody = error?.Message ?? throw new Exception("No message in error response");

            return $"The error \"{e.Message}\" with body \"{errorBody}\"";
        }
        
        catch (Exception)
        {
            return $"The error \"{e.Message}\"";
        }
    }
}

/// <summary>
/// The error body returned by the API.
/// </summary>
/// <param name="Message">The message included in the body</param>
public sealed record ApiErrorBody(string Message);