using Frontend.GenericInterfaces;
using Spectre.Console;

namespace Frontend.Utils;

/// <summary>
/// Present the user with a list of options.
/// </summary>
public static class PresentOptions
{
    /// <summary>
    /// Present the user with a list of options.
    /// </summary>
    public static async Task<IViewController> PresentAsync(params IViewController[] modes)
    {
        return await AnsiConsole.PromptAsync(
            new SelectionPrompt<IViewController>()
                .Title("Choose an option")
                .PageSize(15)
                .UseConverter(controller => controller.ViewName)
                .AddChoices(modes)
        );
    }
}