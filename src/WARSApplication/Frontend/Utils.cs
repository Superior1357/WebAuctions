using Spectre.Console;

namespace Frontend;

public static class Utils
{
    public static Task<DateTimeOffset> PromptTimeAsync(string prompt) => AnsiConsole.PromptAsync(
        new TextPrompt<DateTimeOffset>(prompt).DefaultValue(DateTimeOffset.Now + TimeSpan.FromMinutes(15)).EditableDefaultValue(true)
    );
}