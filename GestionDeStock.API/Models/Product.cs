using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Product : ITimestamped
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Desc { get; set; }
        public required int CategoryId { get; set; }    
        public  Category? Category { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public required int Threshold { get; set; }
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for many-to-many relationship with Supplier
        public List<Supplier> Suppliers { get; set; } = new();
    }
}