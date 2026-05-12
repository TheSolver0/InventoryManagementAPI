using System.ComponentModel.DataAnnotations;

namespace GestionDeStock.API.Dtos
{
    public class ProductDiscountDto
    {
        [Required]
        public int ProductId { get; set; }
        public int? EventId { get; set; }
        /// <summary>Type de réduction : "Percentage" ou "Fixed"</summary>
        [Required]
        [RegularExpression("^(Percentage|Fixed)$", ErrorMessage = "DiscountType doit être 'Percentage' ou 'Fixed'")]
        public required string DiscountType { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La valeur de réduction doit être positive")]
        public decimal DiscountValue { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
