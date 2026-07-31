using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public class User
{
    public UserId Id { get; init; }
    
    public string IdentityId { get; init; } = string.Empty;
}