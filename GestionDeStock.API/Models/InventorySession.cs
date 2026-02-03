using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionDeStock.API.Models
{
    public class InventorySession
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string? Reference { get; set; } // "INV-2026-001"
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? ValidatedDate { get; set; }
        
        public InventoryType Type { get; set; }
        
        public InventoryStatus Status { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? ValidatedBy { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Relations
        public ICollection<InventoryLine> Lines { get; set; } = [];
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
    
    public enum InventoryType
    {
        Full = 0,      // Inventaire complet
        Cyclic = 1,    // Inventaire tournant
        Spot = 2       // Inventaire ciblé
    }
    
    public enum InventoryStatus
    {
        InProgress = 0,  // En cours
        Validated = 1,   // Validé
        Cancelled = 2    // Annulé
    }
}