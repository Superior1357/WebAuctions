using Microsoft.Extensions.DependencyInjection;
using Refit;
using Spectre.Console;

namespace Frontend;

public class ViewController(IServiceProvider serviceProvider) : IViewController
{
    public Task<IViewResult> ShowViewAsync<T>() where T : IView
    {
        var view = serviceProvider.GetRequiredService<T>();
        return ShowViewAsync(view);
    }

    public async Task<IViewResult> ShowViewAsync(IView view)
    {
        try
        {
            return await view.RunAsync();
        }
        catch (ApiException e)
        {
            await ShowApiErrorAsync(e);
            return new Result();
        }
        catch (Exception e)
        {
            await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"The following error: {e.Message} occured. Click to continue...")
            );
            return new Result();
        }
    }

    private static async Task ShowApiErrorAsync(ApiException e)
        => await AnsiConsole.PromptAsync(
            new ConfirmationPrompt($"The following error: {await ExtractMessageAsync(e)} occured. Click to continue...")
        );

    private static async Task<string> ExtractMessageAsync(ApiException e)
    {
        var error = await e.GetContentAsAsync<ApiErrorBody>(); // TODO: error code here
        var errorBody = error?.Message ?? throw new Exception("No message in error response");

        return $"The error \"{e.Message}\" with body \"{errorBody}\" occured";
    }
}


public sealed record ApiErrorBody(string Message);