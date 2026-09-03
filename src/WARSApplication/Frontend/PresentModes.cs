using Spectre.Console;

namespace Frontend;

public static class PresentModes
{
    public static async Task<IProgramMode> PresentAsync(params IProgramMode[] modes)
    {
        return await AnsiConsole.PromptAsync(
            new SelectionPrompt<IProgramMode>()
                .Title("Choose a mode")
                .PageSize(15)
                .UseConverter(mode => mode.ToString())
                .AddChoices(modes)
        );
    }
}