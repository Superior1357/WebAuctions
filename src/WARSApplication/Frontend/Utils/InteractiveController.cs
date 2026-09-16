using Spectre.Console;

namespace Frontend;

public class InteractiveController
{
    public Button? LeftButton { get; set; }
    public Button? RightButton { get; set; }

    public Button? UpButton { get; set; }
    public Button? DownButton { get; set; }

    private static void ConditionalAddButton(Button? button, List<Button> buttons)
    {
        if (button is {} realButton)
        {
            buttons.Add(realButton);    
        }
    }

public async Task MakeSelectionAsync()
    {
        var availableButtons = new List<Button>();
        
        ConditionalAddButton(LeftButton, availableButtons);
        ConditionalAddButton(RightButton, availableButtons);
        ConditionalAddButton(UpButton, availableButtons);
        ConditionalAddButton(DownButton, availableButtons);
        
        var action = await AnsiConsole.PromptAsync(
                new SelectionPrompt<Button>()
                    .Title("Choose an action")
                    .UseConverter(b => b.Appearance)
                    .AddChoices(availableButtons)
            );
        
        action.Action();
    }
}

public readonly record struct Button(Action Action, string Appearance);