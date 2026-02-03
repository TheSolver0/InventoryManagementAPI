using GestionDeStock.API.Models;
namespace GestionDeStock.API.Dtos
{
    public class OrderDto
    {
        public  int Id { get; set; }
        public required int Quantity { get; set; }
        public decimal Amount { get; set; }
        public required int CustomerId { get; set; }
        public required int ProductId { get; set; } = new();
        public  Product? Product { get; set; }
        public  Customer? Customer { get; set; }
        public OrderStatus Status { get; set; } 

    }
}