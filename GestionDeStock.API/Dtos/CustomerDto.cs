namespace GestionDeStock.API.Dtos
{
    public class CustomerDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required int Telephone { get; set; }
       
    }
}
