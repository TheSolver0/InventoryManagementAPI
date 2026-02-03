using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Supplier : ITimestamped
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required  int Telephone { get; set; }    
        public required int Delay { get; set; }
        public List<Product> Products { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    

    }
}