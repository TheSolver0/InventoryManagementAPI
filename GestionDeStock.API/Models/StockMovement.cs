using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class StockMovement : ITimestamped
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Type de mouvement : Ajustement, Transfert, Perte, Retour, etc.
        /// </summary>
        public required string Type { get; set; }
        
        /// <summary>
        /// Quantité (positive ou négative)
        /// </summary>
        public required int Quantity { get; set; }
        
        /// <summary>
        /// Raison du mouvement
        /// </summary>
        public string? Reason { get; set; }
        
        /// <summary>
        /// Référence externe (ex: numéro d'inventaire, bon de transfert)
        /// </summary>
        public string? Reference { get; set; }
        
        // Relations
        public required int ProductId { get; set; }
        public Product? Product { get; set; }
        
        /// <summary>
        /// Lien optionnel vers le Movement commercial si applicable
        /// </summary>
        public int? RelatedMovementId { get; set; }
        public Movement? RelatedMovement { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Utilisateur qui a effectué le mouvement
        /// </summary>
        public string? CreatedBy { get; set; }
    }
}