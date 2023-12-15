using System.Text.Json;

using HurryUpHaul.Domain.Models.Database;

using Microsoft.EntityFrameworkCore;

namespace HurryUpHaul.Domain.Databases
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; init; }
        public DbSet<OrderEvent> OrderEvents { get; init; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<OrderEvent>()
                .Property(e => e.Payload)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<object>(v, (JsonSerializerOptions)null)
                );
        }
    }
}