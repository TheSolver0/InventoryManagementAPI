using System.ComponentModel.DataAnnotations;    

namespace GestionDeStock.API.Models
{
    public class Order : ITimestamped
    {
        public  int Id { get; set; }
        public OrderType  Type { get; set; } = OrderType.ENTREE;
        public  int Quantity { get; set; }
        public  decimal Amount { get; set; }
        public int CustomerId { get; set; }
        public  Customer? Customer { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.EN_ATTENTE; 
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


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