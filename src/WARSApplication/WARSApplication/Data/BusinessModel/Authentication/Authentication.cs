using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WARSApplication.Data.DataModel.Authentication;

internal class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext(options);