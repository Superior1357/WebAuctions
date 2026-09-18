using Backend.Data.SharedDataProperties;

namespace Backend.Data.DataModel;

internal record DataModelUser
{
    public UserId Id { get; init; }
    public string IdentityId { get; init; } = string.Empty;
}