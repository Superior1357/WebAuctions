using Refit;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.Miscellaneous;

public interface IIdentityApi
{
    [Get("/identity")]
    public Task<UserId> RevealUserId();
}