using GestionDeStock.API.Models;

namespace GestionDeStock.API.Dtos
{
    public class SupplierDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required int Telephone { get; set; }
        public required int Delay { get; set; }
        public required List<int> Products { get; set; }
       
    }
}
