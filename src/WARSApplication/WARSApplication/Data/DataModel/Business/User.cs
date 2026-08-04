using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public class User
{
    public UserId Id { get; init; }
    public string IdentityId { get; init; } = string.Empty;
    
    public static async ValueTask<User?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var identityId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityId is null)
            return null;

        var db = context.RequestServices.GetRequiredService<AuctionDbContext>();
        return await db.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
    }
}