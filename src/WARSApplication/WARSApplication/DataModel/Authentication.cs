using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WARSApplication.DataModel;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext(options);