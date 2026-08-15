using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public class User
{
    public UserId Id { get; init; }
    public IEnumerable<Auction> CreatedAuctions => throw new NotImplementedException();
    public IEnumerable<Auction> ParticipatedAuctions => throw new NotImplementedException();
}