using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

/// <summary>
/// Business model for a user. When creating a new user, the object should be created with the default constructor.
/// The restore function should be used to restore an object that has been persisted, for instance, to the database.
/// </summary>
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
        
        return user is not null ? new User { Id = user.Id } : null;
    }
}