using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Frontend;
using Frontend.BuyerMode;
using Frontend.SellerMode;
using Refit;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);
const string address = "http://localhost:5205";

builder.Services
    .AddRefitClient<IBuyMethodsAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{address}/buy"));

builder.Services.AddTransient<Program>();

builder.Services.AddTransient<BuyerRole>();
builder.Services.AddTransient<SellerRole>();
builder.Services.AddTransient<AuthenticationRole>();

builder.Services.AddTransient<CreateMode>();
builder.Services.AddTransient<UpdateMode>();
builder.Services.AddTransient<ListMode>();

builder.Services.AddTransient<ListAllSellersMode>();

using var host = builder.Build();

var runner = host.Services.GetRequiredService<Program>();
await runner.RunAsync();

public partial class Program(IServiceProvider services)
{
    public async Task RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();

            IProgramMode[] menuOptions =
            [
                services.GetRequiredService<BuyerRole>(),
                services.GetRequiredService<SellerRole>(),
            ];
            
            var mode = await PresentModes.PresentAsync(menuOptions);
            _ = await mode.RunAsync();
        }
    }
}