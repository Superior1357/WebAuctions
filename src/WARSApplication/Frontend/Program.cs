using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Frontend;
using Frontend.Authentication;
using Frontend.BuyerMode;
using Frontend.SellerMode;
using Refit;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);
const string address = "http://localhost:5205";

builder.Services
    .AddRefitClient<IBuyMethodsAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{address}/buy"))
    .AddHttpMessageHandler<AuthHandler>()
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{address}/auth"))
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<ISellMethodsAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{address}/sell"))
    .AddHttpMessageHandler<AuthHandler>()
    .RemoveAllLoggers();

builder.Services.AddSingleton<ITokenBearer, TokenBearer>();
builder.Services.AddTransient<Program>();

builder.Services.AddTransient<BuyerRole>();
builder.Services.AddTransient<SellerRole>();

builder.Services.AddTransient<CreateMode>();
builder.Services.AddTransient<UpdateMode>();
builder.Services.AddTransient<ListMode>();

builder.Services.AddTransient<RegisterMode>();
builder.Services.AddTransient<LoginMode>();

builder.Services.AddTransient<AuthHandler>();
builder.Services.AddTransient<ListAllSellersMode>();

builder.Services.AddTransient<IViewController, ViewController>();

using var host = builder.Build();

var runner = host.Services.GetRequiredService<Program>();
await runner.RunAsync();
return;

public partial class Program(IServiceProvider services)
{
    public async Task RunAsync()
    {
        var viewController = services.GetRequiredService<IViewController>();
        var tokenBearer = services.GetRequiredService<ITokenBearer>();
        
        do
        {
            AnsiConsole.Clear();
            
            var authMode = await PresentModes.PresentAsync(
                services.GetRequiredService<LoginMode>(),
                services.GetRequiredService<RegisterMode>()
            );

            await viewController.ShowViewAsync(authMode);
        } while (tokenBearer.Token == null);
        
        while (true)
        {
            AnsiConsole.Clear();

            IView[] menuOptions =
            [
                services.GetRequiredService<BuyerRole>(),
                services.GetRequiredService<SellerRole>()
            ];
            
            var mode = await PresentModes.PresentAsync(menuOptions);
            _ = await viewController.ShowViewAsync(mode);
            
            AnsiConsole.Prompt(
                new TextPrompt<string>("The process is complete. Press [green][[Enter]][/] to continue...")
                    .AllowEmpty()
                    .Secret() // Prevents typed text from showing if they type before hitting Enter
            );
        }
    }
}