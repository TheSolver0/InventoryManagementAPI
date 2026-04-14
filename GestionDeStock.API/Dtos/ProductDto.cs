namespace GestionDeStock.API.Dtos
{
    public class ProductDto
    {
        public required string Name { get; set; }
        public required string Desc { get; set; }
        public required int CategoryId { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public required int Threshold { get; set; }
        public required string? Sku { get; set; }
        public required string? Location { get; set; }
        public string? Brand { get; set; }
        public string? Badge { get; set; }
        public float Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public List<IFormFile>? Images { get; set; }
    }
}
