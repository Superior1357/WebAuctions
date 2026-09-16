using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Frontend.Authentication;
using Frontend.BuyerRole;
using Frontend.GenericInterfaces;
using Frontend.MainMenu;
using Frontend.Miscellaneous;
using Frontend.SellerRole;
using Frontend.Utils;
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
    .AddRefitClient<IBuyMethodsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{apiBaseAddress}/buy"))
    .AddHttpMessageHandler<AuthHandler>()
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{apiBaseAddress}/auth"))
    .RemoveAllLoggers();

builder.Services
    .AddRefitClient<ISellMethodsApi>()
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

builder.Services.AddTransient<ListParticipatedAuctionsViewController>();
builder.Services.AddTransient<ListWonAuctionsViewController>();

builder.Services.AddTransient<AuthenticationViewController>();
builder.Services.AddTransient<MainMenuLoopViewController>();

builder.Services.AddTransient<IMasterController, MasterController>();

using var host = builder.Build();

var runner = host.Services.GetRequiredService<Program>();
await runner.RunAsync();

public partial class Program(IServiceProvider services)
{
    /// <summary>
    /// The main entry point for the application.
    /// Ask the user to login or register and then show the main menu.
    /// </summary>
    public async Task RunAsync()
    {
        var viewController = services.GetRequiredService<IMasterController>();
        
        var authView = services.GetRequiredService<AuthenticationViewController>();
        await viewController.ShowViewAsync(authView);
        
        var mainMenuLoopView = services.GetRequiredService<MainMenuLoopViewController>();
        await viewController.ShowViewAsync(mainMenuLoopView);
    }
}