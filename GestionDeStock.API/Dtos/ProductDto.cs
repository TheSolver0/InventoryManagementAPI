namespace GestionDeStock.API.Dtos
{
    public class ProductDto
    {
        public required string Name { get; set; }
        public required string Desc { get; set; }
        public required int CategoryId { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public required int Seuil { get; set; }
    }
}
