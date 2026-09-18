using Refit;
using Backend.Data.SharedDataProperties;

namespace Frontend.Miscellaneous;

public interface IIdentityApi
{
    [Get("/identity")]
    public Task<UserId> RevealUserId();
}