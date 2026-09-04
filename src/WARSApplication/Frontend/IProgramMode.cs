namespace Frontend;

public interface IView
{
    Task<IViewResult> RunAsync();
}