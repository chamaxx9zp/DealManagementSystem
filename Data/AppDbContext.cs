using Microsoft.EntityFrameworkCore;
using DealManagementSystem.Entities;

namespace DealManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Deal> Deals { get; set; }
        public DbSet<Hotel> Hotels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Deal>()
                .HasIndex(d => d.Slug)
                .IsUnique();

            modelBuilder.Entity<Deal>()
                .HasMany(d => d.Hotels)
                .WithOne(h => h.Deal)
                .HasForeignKey(h => h.DealId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}