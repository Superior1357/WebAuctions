namespace Frontend;

public interface IView
{
    public string Title { get; }
    Task<IViewResult> RunAsync();
}

public interface IViewController
{
    Task<IViewResult> ShowViewAsync<T>() where T : IView;
    Task<IViewResult> ShowViewAsync(IView view);
}