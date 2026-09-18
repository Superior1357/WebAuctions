namespace Frontend.GenericInterfaces;

/// <summary>
/// Interface for all concrete view controllers.
/// </summary>
public interface IViewController
{
    public string ViewName { get; }
    
    /// <summary>
    /// Show the corresponding view.
    /// </summary>
    Task RunAsync();
}

/// <summary>
/// Interface for the master controller.
/// Intended for concrete view control launching and exception handling.
/// </summary>
public interface IMasterController
{
    Task ShowViewAsync(IViewController viewController);
}