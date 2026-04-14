using GestionDeStock.API.Models;

namespace GestionDeStock.API.Dtos
{
    public class UpdateUserDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
