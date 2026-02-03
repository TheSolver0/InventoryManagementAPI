using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDeStock.API.Models
{
    public class InventoryLine
    {
        public int Id { get; set; }
        
        // Clés étrangères
        public int InventorySessionId { get; set; }
        public int ProductId { get; set; }
        
        // Informations dupliquées pour historique (au cas où le produit change)
        [Required]
        [MaxLength(50)]
        public string? ProductSku { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string? ProductName { get; set; }
        
        [MaxLength(100)]
        public string? Location { get; set; }
        
        // Quantités
        [Column(TypeName = "decimal(18,2)")]
        public decimal TheoreticalQuantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CountedQuantity { get; set; }
        
        // Calculé automatiquement
        [NotMapped]
        public decimal Variance => (CountedQuantity ?? 0) - TheoreticalQuantity;
        
        // Traçabilité du comptage
        public DateTime? CountedAt { get; set; }
        
        [MaxLength(100)]
        public string? CountedBy { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Relations de navigation
        public InventorySession Session { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}