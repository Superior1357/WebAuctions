using Spectre.Console;

namespace Frontend;

public static class PresentOptions
{
    public static async Task<IView> PresentAsync(params IView[] modes)
    {
        return await AnsiConsole.PromptAsync(
            new SelectionPrompt<IView>()
                .Title("Choose an option")
                .PageSize(15)
                .UseConverter(mode => mode.Title)
                .AddChoices(modes)
        );
    }
}