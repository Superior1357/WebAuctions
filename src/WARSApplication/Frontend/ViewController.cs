using Microsoft.Extensions.DependencyInjection;
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

        catch (Exception e)
        {
            await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"The following error: {e.Message} occured. Click to continue...")
            );

            return new Result();
        }
    }
}