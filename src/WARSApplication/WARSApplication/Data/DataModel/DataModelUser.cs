using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel;

public record DataModelUser
{
    public UserId Id { get; init; }
    public string IdentityId { get; init; } = string.Empty;
}