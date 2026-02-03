using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Models;

namespace GestionDeStock.API.Data
{
    public static class AppDbContextSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var cat1 = new Category { Id = 1, Title = "Électronique" };
                var cat2 = new Category { Id = 2, Title = "Alimentation" };

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
                        Price = 299.99m,
                        Threshold = 10
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Chocolat",
                        Desc = "Tablette de chocolat noir 70%",
                        CategoryId = cat2.Id,
                        Category = cat2,
                        Quantity = 200,
                        Price = 2.99m,
                        Threshold = 30
                    }
                );

                await context.SaveChangesAsync();
            }
            if (!context.Suppliers.Any())
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
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer
                    {
                        Id = 1,
                        Name = "Client A",
                        Email = "contact@clienta.com",
                        Address = "1 Rue de la Paix, Paris",
                        Telephone = 123456789,
                        Points = 100
                    },
                    new Customer
                    {
                        Id = 2,
                        Name = "Client B",
                        Email = "contact@clientb.com",
                        Address = "2 Avenue des Champs-Élysées, Paris",
                        Telephone = 987654321,
                        Points = 100
                    }
                );

                await context.SaveChangesAsync();
            }
        }

    }
}

                