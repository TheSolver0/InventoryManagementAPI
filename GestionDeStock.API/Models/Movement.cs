namespace GestionDeStock.API.Models
{
    public class Movement
    {
        public required int Id { get; set; }
        public required string Type { get; set; }
        public required string Quantity { get; set; }
        public  int CustomerId { get; set; }    
        public  Customer? Customer { get; set; }
        public  int SupplierId { get; set; }    
        public  Supplier? Supplier { get; set; }
        public  int ProductId { get; set; }    
        public required Product Product { get; set; }     

    }
}