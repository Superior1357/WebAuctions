using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record User
{
    public UserId Id { get; init; }
}