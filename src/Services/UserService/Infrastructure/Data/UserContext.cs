using Microsoft.EntityFrameworkCore;
using RateWatch.UserService.Domain.Entities;

namespace RateWatch.UserService.Infrastructure.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
