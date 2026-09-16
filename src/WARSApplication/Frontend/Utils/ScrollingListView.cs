using Spectre.Console;

namespace Frontend.Utils;

/// <summary>
/// A scrolling list view controller that allows the user to navigate through a list.
/// </summary>
/// <param name="header"></param>
/// <param name="items"></param>
/// <param name="itemToPanel"></param>
/// <typeparam name="T"></typeparam>
public class ScrollingListViewController<T>(string header, List<T> items, Func<T, Panel> itemToPanel) where T : class
{
    private int _currentIndex = 0;
    private bool _abort = false;
    private T? _result;
    
    private void RightAction()
    {
        _result = items[_currentIndex];
    }

    private void LeftAction()
    {
        _abort = true;
    }

    private void DownAction()
    {
        if (_currentIndex < items.Count - 1) _currentIndex++;
    }

    private void UpAction()
    {
        if (_currentIndex > 0) _currentIndex--;
    }
    
    /// <summary>
    /// Make the user select an item from the list.
    /// Allows for browsing through the list via Up and Down buttons.
    /// Allows for aborting the selection via the Quit button.
    /// Allows for selection via the Submit button.
    /// </summary>
    /// <returns>
    /// Null if aborted.
    /// The selected item otherwise.
    /// </returns>
    public async Task<T?> MakeChoiceAsync()
    {
        if (items.Count == 0) return null;

        var controller = new InteractiveController
        {
            LeftButton = new Button(LeftAction, "Quit"),
            RightButton = new Button(RightAction, "Submit"),
            UpButton = new Button(UpAction, "Up"),
            DownButton = new Button(DownAction, "Down")
        };
        
        while (!_abort && _result == null)
        {
            AnsiConsole.Clear();
            var shownPanel = new Panel(itemToPanel(items[_currentIndex]));
            AnsiConsole.Write(shownPanel);
            
            AnsiConsole.WriteLine(header);
            await controller.MakeSelectionAsync();
        }
        AnsiConsole.Clear();

        return _result;
    }
    
    
}