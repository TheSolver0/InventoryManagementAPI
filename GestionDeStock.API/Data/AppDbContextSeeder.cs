using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Models;

namespace GestionDeStock.API.Data
{
    public static class AppDbContextSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var cat1 = new Category { Id = 1, Title = "Électronique", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                var cat2 = new Category { Id = 2, Title = "Alimentation", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };

                context.Categories.AddRange(cat1, cat2);

                context.Products.AddRange(
                    new Product
                    {
                        Id = 1,
                        Name = "Smartphone",
                        Desc = "Téléphone Android haut de gamme",
                        CategoryId = cat1.Id,
                        Category = cat1,
                        Quantity = 50,
                        Price = 60000,
                        Threshold = 10,
                        Sku = "SMRT-PHN-001",
                        Location = "Aisle 1, Shelf A",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Chocolat",
                        Desc = "Tablette de chocolat noir 70%",
                        CategoryId = cat2.Id,
                        Category = cat2,
                        Quantity = 200,
                        Price = 2500,
                        Threshold = 30,
                        Sku = "CHOC-NOIR-70",
                        Location = "Aisle 3, Shelf B",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                );

                await context.SaveChangesAsync();
            }
            if (!await context.Suppliers.AnyAsync())
            {
                context.Suppliers.AddRange(
                    new Supplier
                    {
                        Id = 1,
                        Name = "Fournisseur A",
                        Email = "contact@fournisseura.com",
                        Address = "1 Rue de la Paix, Paris",
                        Telephone = 123456789,
                        Delay = 5
                    },
                    new Supplier
                    {
                        Id = 2,
                        Name = "Fournisseur B",
                        Email = "contact@fournisseurb.com",
                        Address = "2 Avenue des Champs-Élysées, Paris",
                        Telephone = 987654321,
                        Delay = 10
                    }
                );

                await context.SaveChangesAsync();
            }
            if (!await context.Customers.AnyAsync())
            {
                context.Customers.AddRange(
                    new Customer
                    {
                        Id = 1,
                        Name = "Client A",
                        Email = "contact@clienta.com",
                        Address = "1 Rue de la Paix, Paris",
                        Telephone = 123456789,
                        Points = 100,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Customer
                    {
                        Id = 2,
                        Name = "Client B",
                        Email = "contact@clientb.com",
                        Address = "2 Avenue des Champs-Élysées, Paris",
                        Telephone = 987654321,
                        Points = 100,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                );

                await context.SaveChangesAsync();
            }
        }

    }
}

                