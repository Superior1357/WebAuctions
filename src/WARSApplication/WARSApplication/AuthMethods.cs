using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WARSApplication.Data.DataModel;
using WARSApplication.Data.DataModel.Business;

namespace WARSApplication;

internal static class AuthMethods
{ // TODO: what if the user exists already?
    /// <summary>
    /// Adds a new user to the authentication database. 
    /// </summary>
    /// <param name="req">the registration API request</param>
    /// <param name="userManager">the user manager</param>
    /// <param name="dbContext">the non-authentication database context</param>
    /// <returns>
    /// OK status code upon success.
    /// BadRequest code and errors upon failure (for example, when the user already exists).
    /// </returns>
    public static async Task<IResult> Register(RegisterRequest req, UserManager<IdentityUser> userManager, AuctionDbContext dbContext)
    {
        var user = new IdentityUser { UserName = req.UserName };
        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded) return Results.BadRequest(result.Errors);
        
        await dbContext.Users.AddAsync(
            new DataModelUser
            {
                IdentityId = user.Id
            }
        );
        
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    
    public static async Task<IResult> Login(LoginRequest req, UserManager<IdentityUser> userManager, IConfiguration config)
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
        
        return Results.Ok(new TokenResponse(jwtString));
    }
}

public record RegisterRequest(string UserName, string Password);
public record LoginRequest(string UserName, string Password);
public record TokenResponse(string Token);