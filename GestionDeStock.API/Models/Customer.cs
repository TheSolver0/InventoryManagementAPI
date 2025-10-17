namespace GestionDeStock.API.Models
{
    public class Customer
    {
        public  int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public  int Telephone { get; set; }    
        public int Points { get; set; }
        public List<Order> Orders { get; set; } = new();
    }
}