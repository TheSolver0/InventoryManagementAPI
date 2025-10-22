using System.ComponentModel.DataAnnotations;

namespace GestionDeStock.API.Models
{
    public class Provide : ITimestamped
    {
        public int Id { get; set; }
        public ProvideType Type { get; set; } = ProvideType.ENTREE;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public ProvideStatus Status { get; set; } = ProvideStatus.EN_ATTENTE;
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


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