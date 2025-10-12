namespace GestionDeStock.API.Models
{
    public class Category
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public List<Product> Products { get; set; } = new();    
    }
}