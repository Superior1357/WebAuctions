namespace Frontend;

public interface IProgramMode
{
    Task<IModeResult> RunAsync();
}