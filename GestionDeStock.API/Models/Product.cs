using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Product : ITimestamped
    {
        public required int Id { get; set; }
        public required string Name { get; set; }        // nom
        public required string Desc { get; set; }        // description
        public required int CategoryId { get; set; }     // categorie_id
        public Category? Category { get; set; }
        public required int Quantity { get; set; }       // stock
        public required decimal Price { get; set; }      // prix
        public decimal? OldPrice { get; set; }           // ancien_prix 
        public required int Threshold { get; set; }
        public required string Sku { get; set; } = string.Empty;
        public required string Location { get; set; } = string.Empty;
        public string? ImagePath { get; set; }           // image 
        public string? Brand { get; set; }               // marque 
        public string? Badge { get; set; }               // badge 
        public float Rating { get; set; } = 0;           // note 
        public int ReviewCount { get; set; } = 0;        // nb_avis 
        public bool IsActive { get; set; } = true;       // actif 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<ProductImage> Images { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<ProductDiscount> Discounts { get; set; } = new();
    }
}