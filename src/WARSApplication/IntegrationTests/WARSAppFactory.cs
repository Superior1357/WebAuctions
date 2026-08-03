using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WARSApplication.Data.DataModel.Business;

namespace IntegrationTests;

public class TestingWARSAppFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _dbConnection = null!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _dbConnection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True;");
        _dbConnection.Open();

        builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
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

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;
        
        _dbConnection.Close();
        _dbConnection.Dispose();
    }
}