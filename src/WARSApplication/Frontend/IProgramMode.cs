namespace Frontend;

public interface IView
{
    Task<IViewResult> RunAsync();
}

public interface IViewController
{
    Task<IViewResult> ShowViewAsync<T>() where T : IView;
    Task<IViewResult> ShowViewAsync(IView view);
}