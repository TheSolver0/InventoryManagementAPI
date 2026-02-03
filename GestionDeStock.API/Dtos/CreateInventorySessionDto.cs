using GestionDeStock.API.Models;

namespace GestionDeStock.API.Dtos
{
    public class CreateInventorySessionDto
    {
        public InventoryType Type { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? LocationIds { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}