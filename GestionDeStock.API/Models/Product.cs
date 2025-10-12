namespace GestionDeStock.API.Models
{
    public class Product
    {
        public  int Id { get; set; }
        public required string Name { get; set; }
        public required string Desc { get; set; }
        public  int CategoryId { get; set; }    
        public  Category? Category { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public required int Seuil { get; set; }
    }
}