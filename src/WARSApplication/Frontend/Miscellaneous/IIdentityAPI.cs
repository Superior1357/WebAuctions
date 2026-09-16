using Refit;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend;

public interface IIdentityApi
{
    [Get("/identity")]
    public Task<UserId> RevealUserId();
}