using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
    public class Movement : ITimestamped
    {
        public required int Id { get; set; }
        public  string? Type { get; set; }
        public  int? Quantity { get; set; }
        public  decimal? Amount { get; set; }
        public  int? CustomerId { get; set; }    
        public  Customer? Customer { get; set; }
        public  int? SupplierId { get; set; }    
        public  Supplier? Supplier { get; set; }
        public required int ProductId { get; set; }    
        public  Product? Product { get; set; }     
        public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
}