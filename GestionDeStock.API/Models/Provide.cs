namespace GestionDeStock.API.Models
{
    public class Provide
    {
        public required int Id { get; set; }
        public required string Type { get; set; }
        public required string Quantity { get; set; }
        public List<Product> Products { get; set; } = new();    

    }
}