namespace GestionDeStock.API.Models
{
    public class Order
    {
        public required int Id { get; set; }
        public required string Type { get; set; }
        public required string Quantity { get; set; }
        public  int CustomerId { get; set; }    
        public required Customer Customer { get; set; }
        public List<Product> Products { get; set; } = new();    

    }
}