using System.ComponentModel.DataAnnotations;

namespace GestionDeStock.API.Dtos
{
    public class PromotionEventDto
    {
        [Required]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? BadgeLabel { get; set; }
        public string? BgColor { get; set; }
        public string? TextColor { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
