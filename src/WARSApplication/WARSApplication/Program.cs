using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using WARSApplication;
using WARSApplication.APIData;
using WARSApplication.Repository;
using WARSApplication.DataModel;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Auctions") ?? "Data Source=Auctions.db";
var authConnectionString = builder.Configuration.GetConnectionString("Auth") ?? "Data Source=Auth.db";

builder.Services.AddSqlite<AuthDbContext>(authConnectionString);
builder.Services.AddSqlite<AuctionDbContext>(connectionString);
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

builder.Services.AddIdentityCore<IdentityUser>(
    options =>
    {
        // Relax password rules for easier testing
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    }
    ).AddEntityFrameworkStores<AuthDbContext>();

var jwtKey = builder.Configuration["JwtSecret"]; // TODO: what if not configured?

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,  // Set to true in production if you define an issuer
            ValidateAudience = false // Set to true in production if you define an audience
        };
    });

builder.Services.AddAuthorization();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapGroup("/buy").MapBuyApis();
app.MapGroup("/sell").MapSellApis().RequireAuthorization();
app.MapGroup("/authenticate").MapAuthApis();

/*
var summaries = new[]
   {
       "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    }; 
 
app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");
    
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
   {
       public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
   }
*/


app.Run();

internal static class ApiMappingExtensions {
    extension(RouteGroupBuilder group)
    {
        public RouteGroupBuilder MapBuyApis()
        {
            // TODO
            return group;
        }

        public RouteGroupBuilder MapSellApis()
        {
            // TODO
            group.MapPost("/new", AuctionManagement.CreateAuctionAsync);
            group.MapPatch("/update", AuctionManagement.UpdateAuction);
            
            group.MapGet("/list", AuctionManagement.ListAuctionsAsync).Produces<Auction[]>();
            group.MapGet("/info/{id}", AuctionManagement.AuctionInfo).Produces<AuctionInfo>();
            return group;
        }

        public RouteGroupBuilder MapAuthApis()
        { // TODO: check if really working (changed emails to username, deleted email from line 96)
            group.MapPost("/register", async (RegisterRequest req, UserManager<IdentityUser> userManager) =>
            {
                var user = new IdentityUser { UserName = req.UserName };
                var result = await userManager.CreateAsync(user, req.Password);
                
                return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
            });

            group.MapPost("/login", async (LoginRequest req, UserManager<IdentityUser> userManager, IConfiguration config) =>
            {
                var user = await userManager.FindByNameAsync(req.UserName);
                if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
                    return Results.Unauthorized();
                
                // 1. Get the secret key bytes
                var keyBytes = Encoding.UTF8.GetBytes(config["JwtSecret"]!);
                
                // 2. Define the token's properties (Claims, Expiration, Signature)
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity([
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    ]),
                    Expires = DateTime.UtcNow.AddHours(8),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(keyBytes), 
                        SecurityAlgorithms.HmacSha256Signature)
                };
                
                // 3. Generate the token string
                var handler = new JwtSecurityTokenHandler();
                var token = handler.CreateToken(tokenDescriptor);
                var jwtString = handler.WriteToken(token);

                return Results.Ok(new { Token = jwtString });
            });
            return group;
        }
    }
}

public record RegisterRequest(string UserName, string Password);
public record LoginRequest(string UserName, string Password);