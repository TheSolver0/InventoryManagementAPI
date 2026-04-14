namespace GestionDeStock.API.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public required string ImagePath { get; set; }
        public bool IsMain { get; set; } = false;
        public int Order { get; set; } = 0;
    }
}
