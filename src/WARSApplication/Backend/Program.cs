using System.Text;
using Backend.BusinessMethods;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Backend.Repository;
using Backend;
using Backend.AuctionEvaluation;
using Backend.Data;
using Backend.Data.APIData.Outgoing;
using Backend.Data.BusinessModel.Authentication;
using Backend.Data.BusinessModel.Business;
using Backend.Data.DataConverters;
using Backend.Utils;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "Databases");
Directory.CreateDirectory(dataDirectory);

var connectionString = builder.Configuration.GetConnectionString("Auctions")
                       ?? $"Data Source={Path.Combine(dataDirectory, "Auctions.db")}";
var authConnectionString = builder.Configuration.GetConnectionString("Auth")
                           ?? $"Data Source={Path.Combine(dataDirectory, "Auth.db")}";

builder.Services.AddSqlite<AuthDbContext>(authConnectionString);

builder.Services.AddDbContext<AuctionDbContext>(
        options => options
            .UseSqlite(connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddScoped<IRepository<Auction>, AppRepository>();
builder.Services.AddScoped<ICurrentTime, CurrentTime>();
builder.Services.AddScoped<IAuctionEvaluator, AuctionEvaluator>();
builder.Services.AddScoped<IDataConverter<Auction, SellerAuctionInfo>, AuctionToSellerAuctionInfo>();
builder.Services.AddScoped<IDataConverter<Auction, BuyerAuctionInfo>, BusinessToBuyerAuctionInfo>();

builder.Services.AddIdentityCore<IdentityUser>(
    options =>
    {
        // Relax password rules
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 1;
    }
).AddEntityFrameworkStores<AuthDbContext>();

var secretFile = Path.Combine(dataDirectory, "jwt_secret");
var jwtKey = File.Exists(secretFile) ? File.ReadAllText(secretFile).Trim() : throw new Exception("JwtSecret not configured"); 
builder.Configuration["JwtSecret"] = jwtKey;

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
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "V1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapGroup("/buy").MapBuyApis().RequireAuthorization();
app.MapGroup("/sell").MapSellApis().RequireAuthorization();
app.MapGet("/identity", AuthMethods.RevealUserId).RequireAuthorization();
app.MapGroup("/auth").MapAuthApis();

app.Run();

namespace Backend
{
    internal static class ApiMappingExtensions {
        extension(RouteGroupBuilder group)
        {
            public RouteGroupBuilder MapBuyApis()
            {
                group.MapPost("/bid", BuyMethods.BidAsync);
                group.MapGroup("/list").MapBuyListApis();
                group.MapPost("/info", BuyMethods.GetInfoAsync); // information about auctions
                
                return group;
            }

            private void MapBuyListApis()
            {
                group.MapGet("/sellers", BuyMethods.ListAllSellersAsync);
                group.MapPost("/not_finished", BuyMethods.ListNotFinishedAuctionsOfASellerAsync);
                
                group.MapGet("/won", BuyMethods.ListWonAuctionsAsync);
                group.MapGet("/participated", BuyMethods.ListParticipatedAuctionsAsync);
            }

            public RouteGroupBuilder MapSellApis()
            {
                group.MapPost("/create", SellMethods.CreateAuctionAsync);
                group.MapPatch("/update", SellMethods.UpdateAuctionAsync);
                
                group.MapGet("/list/{status}", SellMethods.ListAsync);
                group.MapPost("/info", SellMethods.GetInfoAsync);
                
                return group;
            }
            
            public void MapAuthApis()
            {
                group.MapPost("/register", AuthMethods.Register);
                group.MapPost("/login", AuthMethods.Login);
            }
        }
    }
} // TODO: check warnings