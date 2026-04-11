using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Category : ITimestamped
    {
        public  int Id { get; set; }
        public required string Title { get; set; }
        public List<Product> Products { get; set; } = [];    
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}