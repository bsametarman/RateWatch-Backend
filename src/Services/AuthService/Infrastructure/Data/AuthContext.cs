using Microsoft.EntityFrameworkCore;
using RateWatch.AuthService.Domain.Entities;

namespace RateWatch.AuthService.Infrastructure.Data
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options)
        {
        }

        public DbSet<AuthUser> AuthUsers { get; set; }
    }
}
