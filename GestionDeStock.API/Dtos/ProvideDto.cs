using GestionDeStock.API.Models;
namespace GestionDeStock.API.Dtos
{
    public class ProvideDto
    {
        public required int Id { get; set; }
        public required int Quantity { get; set; }
        public required decimal Amount { get; set; }
        public required int SupplierId { get; set; }
        public required int ProductId { get; set; } = new();
        public  Product? Product { get; set; }
        public  Supplier? Supplier { get; set; }
        public required ProvideStatus Status { get; set; } 

    }
}