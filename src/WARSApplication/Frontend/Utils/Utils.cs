using Spectre.Console;

namespace Frontend.Utils;

public static class Utils
{
    /// <summary>
    /// Prompt the user for a time.
    /// </summary>
    /// <param name="prompt">The prompt to show</param>
    /// <returns>The input time</returns>
    public static Task<DateTimeOffset> PromptTimeAsync(string prompt) => AnsiConsole.PromptAsync(
        new TextPrompt<DateTimeOffset>(prompt).DefaultValue(DateTimeOffset.Now + TimeSpan.FromMinutes(15)).EditableDefaultValue(true)
    );
    
    /// <summary>
    /// Prompt the user to press Enter to continue.
    /// </summary>
    public static void MakeConfirmationPrompt()
    {
        AnsiConsole.Prompt(
            new TextPrompt<string>("The process is complete. Press [green][[Enter]][/] to continue...")
                .AllowEmpty()
                .Secret() // Prevents typed text from showing if they type before hitting Enter
        );
    }
}