namespace GestionDeStock.API.Models
{
    public class Supplier
    {
        public  int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public  int Telephone { get; set; }    
        public  int Delay { get; set; }    
        public List<Product> Products { get; set; } = new();    

    }
}