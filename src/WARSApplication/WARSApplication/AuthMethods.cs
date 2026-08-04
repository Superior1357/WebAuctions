using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace WARSApplication;

public static class AuthMethods
{ // TODO: check if really working (changed emails to username, deleted email from line 96)
    public static async Task<IResult> Register(RegisterRequest req, UserManager<IdentityUser> userManager)
    {
        var user = new IdentityUser { UserName = req.UserName };
        var result = await userManager.CreateAsync(user, req.Password);
                
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
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

        return Results.Ok(new { Token = jwtString });
    }
    
    public abstract record RegisterRequest(string UserName, string Password);
    public abstract record LoginRequest(string UserName, string Password);
}
