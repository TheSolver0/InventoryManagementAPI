namespace GestionDeStock.API.Models
{
    public class Movement : ITimestamped
    {
        public  int Id { get; set; }
        public  string? Type { get; set; }
        public  int? Quantity { get; set; }
        public  decimal? Amount { get; set; }
        public  int? CustomerId { get; set; }    
        public  Customer? Customer { get; set; }
        public  int? SupplierId { get; set; }    
        public  Supplier? Supplier { get; set; }
        public  int ProductId { get; set; }    
        public  Product? Product { get; set; }     
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


    }
}