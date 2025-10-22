namespace GestionDeStock.API.Models
{
    public class Category : ITimestamped
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public List<Product> Products { get; set; } = new();    
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}