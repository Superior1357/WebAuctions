using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WARSApplication.Repository;
using WARSApplication;
using WARSApplication.BusinessMethods;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Authentication;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

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
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapGroup("/buy").MapBuyApis().RequireAuthorization();
app.MapGroup("/sell").MapSellApis().RequireAuthorization();
app.MapGroup("/authenticate").MapAuthApis();

app.Run();

namespace WARSApplication
{
    internal static class ApiMappingExtensions {
        extension(RouteGroupBuilder group)
        {
            public RouteGroupBuilder MapBuyApis()
            {
                group.MapPost("/bid", BuyMethods.BidAsync);
                group.MapGroup("/list").MapBuyListApis(); // just ids
                group.MapGet("/info", BuyMethods.GetInfoAsync).Produces<IEnumerable<BuyerAuctionInfo>>(); // information about auctions
                
                return group;
            }

            private void MapBuyListApis()
            {
                group.MapGet("/sellers", BuyMethods.ListAllSellersAsync).Produces<IEnumerable<UserId>>();
                group.MapGet("/{auctionType}", BuyMethods.ListPendingAuctionsAsync).Produces<IEnumerable<AuctionId>>();
                
                group.MapGet("/won", BuyMethods.ListWonAuctionsAsync).Produces<IEnumerable<AuctionId>>();
                group.MapGet("/participated", BuyMethods.ListParticipatedAuctionsAsync).Produces<IEnumerable<AuctionId>>();
            }

            public RouteGroupBuilder MapSellApis()
            {
                group.MapPost("/create", SellMethods.CreateAuctionAsync);
                group.MapPatch("/update", SellMethods.UpdateAuction);
                
                group.MapGet("/list/{auctionType}", SellMethods.ListAsync).Produces<IEnumerable<AuctionId>>();
                group.MapGet("/info", SellMethods.GetInfoAsync).Produces<IEnumerable<SellerAuctionInfo>>();
                
                return group;
            }
            
            public void MapAuthApis()
            {
                group.MapPost("/register", AuthMethods.Register);
                group.MapPost("/login", AuthMethods.Login);
            }
        }
    }
}