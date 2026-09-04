using Spectre.Console;

namespace Frontend;

public static class PresentModes
{
    public static async Task<IView> PresentAsync(params IView[] modes)
    {
        return await AnsiConsole.PromptAsync(
            new SelectionPrompt<IView>()
                .Title("Choose a mode")
                .PageSize(15)
                .UseConverter(mode => mode.ToString())
                .AddChoices(modes)
        );
    }
}