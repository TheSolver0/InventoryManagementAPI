using System.ComponentModel.DataAnnotations;
using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Provide : ITimestamped
    {
        public required int Id { get; set; }
        public ProvideType Type { get; set; } = ProvideType.ENTREE;
        public required int Quantity { get; set; }
        public required decimal Amount { get; set; }
        public required int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public required int ProductId { get; set; }
        public Product? Product { get; set; }
        public ProvideStatus Status { get; set; } = ProvideStatus.EN_ATTENTE;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
    public enum ProvideStatus
    {
        [Display(Name = "En attente")]
        EN_ATTENTE,

        [Display(Name = "Préparée")]
        PREPAREE,

        [Display(Name = "Expédiée")]
        EXPEDIEE,

        [Display(Name = "Livrée")]
        LIVREE,

        [Display(Name = "Annulée")]
        ANNULEE
    }
    public enum ProvideType
    {
        [Display(Name = "Entrée")]
        ENTREE,

        [Display(Name = "Sortie")]
        SORTIE,


    }
}