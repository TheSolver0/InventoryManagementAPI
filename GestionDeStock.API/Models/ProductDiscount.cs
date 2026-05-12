using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class ProductDiscount : ITimestamped
    {
        public int Id { get; set; }
        public required int ProductId { get; set; }
        public Product? Product { get; set; }
        public int? EventId { get; set; }
        public PromotionEvent? Event { get; set; }
        public required DiscountType DiscountType { get; set; }
        public required decimal DiscountValue { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
