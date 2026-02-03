namespace GestionDeStock.API.Models
{
    public class Product : ITimestamped
    {
        public  int Id { get; set; }
        public required string Name { get; set; }
        public required string Desc { get; set; }
        public  int CategoryId { get; set; }    
        public  Category? Category { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public required int Threshold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property for many-to-many relationship with Supplier
        public List<Supplier> Suppliers { get; set; } = new();
    }
}