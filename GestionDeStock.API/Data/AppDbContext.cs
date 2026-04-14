using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Models;
using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Provide> Provides { get; set; }
        public DbSet<Movement> Movements { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<InventorySession> InventorySessions { get; set; }
        public DbSet<InventoryLine> InventoryLines { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<HeroSlide> HeroSlides { get; set; }
        public DbSet<User> Users { get; set; }

        

        // Indique si on tourne sur SQLite ou MySQL
          private bool IsSQLite => options.Extensions
        .Any(e => e.GetType().Name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));


        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite exige INTEGER (pas int) pour AUTOINCREMENT → on unifie pour les deux
            configurationBuilder
                .Properties<int>()
                .HaveColumnType("INTEGER");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var prop in entity.GetProperties())
                {
                    // ── DATES ──────────────────────────────────────────────
                    if (prop.ClrType == typeof(DateTime) || prop.ClrType == typeof(DateTime?))
                    {
                        // SQLite : TEXT ISO 8601 | MySQL : DATETIME
                        prop.SetColumnType(IsSQLite ? "TEXT" : "DATETIME");
                    }

                    // ── DECIMALS ───────────────────────────────────────────
                    if (prop.ClrType == typeof(decimal) || prop.ClrType == typeof(decimal?))
                    {
                        prop.SetColumnType("decimal(10,2)");
                    }

                    // ── AUTO INCREMENT (Id int) ────────────────────────────
                    if (prop.Name == "Id" && prop.ClrType == typeof(int))
                    {
                        // SQLite : INTEGER PRIMARY KEY = autoincrement automatique
                        // MySQL  : INT AUTO_INCREMENT
                        prop.SetColumnType(IsSQLite ? "INTEGER" : "INT");
                        prop.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                    }
                }
            }

            // ── RELATIONS ──────────────────────────────────────────────────

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.Products)
                .WithMany(p => p.Suppliers)
                .UsingEntity("SupplierProducts");

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryLine>()
                .HasOne(il => il.Session)
                .WithMany(s => s.Lines)
                .HasForeignKey(il => il.InventorySessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryLine>()
                .HasOne(il => il.Product)
                .WithMany()
                .HasForeignKey(il => il.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }

        // ── Méthode mutualisée pour les timestamps ─────────────────────────
        private void ApplyTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ITimestamped &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (ITimestamped)entry.Entity;
                if (entry.State == EntityState.Added)
                    entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}