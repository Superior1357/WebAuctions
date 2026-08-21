using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record User
{
    public UserId Id { get; private init; }
    public static User Restore(UserId id) => new() { Id = id };
    
    public static async ValueTask<User?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var identityId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityId is null)
            return null;

        var db = context.RequestServices.GetRequiredService<AuctionDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
        return new User { Id = user.Id };
    }
}