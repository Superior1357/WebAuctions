using Spectre.Console;

namespace Frontend;

public static class Utils
{
    public static Task<DateTimeOffset> PromptTimeAsync(string prompt) => AnsiConsole.PromptAsync(
        new TextPrompt<DateTimeOffset>(prompt).DefaultValue(DateTimeOffset.Now + TimeSpan.FromMinutes(15)).EditableDefaultValue(true)
    );
    
    public static void MakeConfirmationPrompt()
    {
        AnsiConsole.Prompt(
            new TextPrompt<string>("The process is complete. Press [green][[Enter]][/] to continue...")
                .AllowEmpty()
                .Secret() // Prevents typed text from showing if they type before hitting Enter
        );
    }
}