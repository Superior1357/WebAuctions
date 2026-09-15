using Spectre.Console;

namespace Frontend;

public class ScrollingListView<T>(string header, List<T> items, Func<T, Panel> itemToPanel) where T : class
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
    
    public async Task<T?> MakeChoiceAsync()
    {
        if (items.Count == 0) return null;

        var controller = new InteractiveController
        {
            LeftButton = new Button(LeftAction, "quit"),
            RightButton = new Button(RightAction, "submit"),
            UpButton = new Button(UpAction, "up"),
            DownButton = new Button(DownAction, "down")
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