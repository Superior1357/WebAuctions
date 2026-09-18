using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.BusinessModel.Authentication;

internal class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext(options);