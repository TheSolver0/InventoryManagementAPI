using System.ComponentModel.DataAnnotations;    
using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Order : ITimestamped
    {
        public required int Id { get; set; }
        public OrderType  Type { get; set; } = OrderType.ENTREE;
        public required int Quantity { get; set; }
        public required decimal Amount { get; set; }
        public required int CustomerId { get; set; }
        public  Customer? Customer { get; set; }
        public required int ProductId { get; set; }
        public Product? Product { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.EN_ATTENTE; 
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
    public enum OrderStatus
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
    public enum OrderType
    {
        [Display(Name = "Entrée")]
        ENTREE,

        [Display(Name = "Sortie")]
        SORTIE,

       
    }
}