using Backend.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace IntegrationTests;

public class TestingWarsAppFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _dbConnection = null!;
    public readonly DateTimeOffset TestApplicationTime = new (2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
        
        _dbConnection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True;");
        _dbConnection.Open();
        
        builder.ConfigureTestServices(services =>
            {
                var timeProviderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TimeProvider));
                if (timeProviderDescriptor != null)
                {
                    services.Remove(timeProviderDescriptor);
                }
                
                var fakeTimeProvider = new FakeTimeProvider(TestApplicationTime);
                services.AddSingleton<TimeProvider>(fakeTimeProvider);
                
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                        options.DefaultScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, MinimalDummyAuthHandler>("Test", _ => { });
                
                var dbDescriptor =
                    services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

                if (dbDescriptor != null)
                {
                    services.Remove(dbDescriptor);
                }

                services.AddDbContext<AuctionDbContext>(options => options.UseSqlite(_dbConnection));
                
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                
                var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
                db.Database.EnsureCreated();
            }
        );
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;
        
        _dbConnection.Close();
        _dbConnection.Dispose();
    }
}