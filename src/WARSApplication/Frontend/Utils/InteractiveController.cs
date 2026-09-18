using Spectre.Console;

namespace Frontend.Utils;

/// <summary>
/// A controller that exposes an interactive interface of four buttons, each of which triggers an action.
/// The buttons are represented by a string which is shown to the user.
/// </summary>
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
    
    /// <summary>
    /// Make the user select an action from the four buttons.
    /// </summary>
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

/// <summary>
/// A button that triggers an action.
/// </summary>
/// <param name="Action">the action when the button is pressed</param>
/// <param name="Appearance">the appearance of the button</param>
public readonly record struct Button(Action Action, string Appearance);