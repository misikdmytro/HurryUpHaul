using HurryUpHaul.Domain.Models.Database;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HurryUpHaul.Domain.Databases
{
    internal class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Order> Orders { get; init; }
        public DbSet<Restaurant> Restaurants { get; init; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.Managers)
                .WithMany();
        }
    }
}