using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Models;

namespace GestionDeStock.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Provide> Provides { get; set; }
        public DbSet<Movement> Movements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Category -> Product (1:N)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer -> Order (1:N)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product <-> Supplier (N:M)
            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.Products)
                .WithMany(p => p.Suppliers)
                .UsingEntity(j => j.ToTable("SupplierProducts"));
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var timestampedEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is ITimestamped &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in timestampedEntries)
            {
                var entity = (ITimestamped)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            var timestampedEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is ITimestamped &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in timestampedEntries)
            {
                var entity = (ITimestamped)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

    }
}