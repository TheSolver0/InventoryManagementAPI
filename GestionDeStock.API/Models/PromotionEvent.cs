using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class PromotionEvent : ITimestamped
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? BannerImage { get; set; }
        public string? BadgeLabel { get; set; }
        public string? BgColor { get; set; }
        public string? TextColor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<ProductDiscount> Discounts { get; set; } = new();
    }
}
