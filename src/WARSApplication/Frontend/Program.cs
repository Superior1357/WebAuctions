using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Frontend;
using Frontend.Authentication;
using Frontend.BuyerMode;
using Frontend.SellerMode;
using Refit;

var builder = Host.CreateApplicationBuilder(args);
var apiBaseAddress = builder.Configuration["Api:BaseAddress"]
                              ?? throw new InvalidOperationException("'Api:BaseAddress' is missing from configuration.");

builder.Services
    .AddRefitClient<IIdentityApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{apiBaseAddress}"))
    .AddHttpMessageHandler<AuthHandler>()
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<IBuyMethodsAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{apiBaseAddress}/buy"))
    .AddHttpMessageHandler<AuthHandler>()
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{apiBaseAddress}/auth"))
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<ISellMethodsAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{apiBaseAddress}/sell"))
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

builder.Services.AddTransient<ListParticipatedAuctionsView>();
builder.Services.AddTransient<ListWonAuctionsView>();

builder.Services.AddTransient<AuthenticationView>();
builder.Services.AddTransient<MainMenuLoopView>();

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
        
        var authView = services.GetRequiredService<AuthenticationView>();
        _ = await viewController.ShowViewAsync(authView);
        
        var mainMenuLoopView = services.GetRequiredService<MainMenuLoopView>();
        _ = await viewController.ShowViewAsync(mainMenuLoopView);
    }
}