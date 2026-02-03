namespace GestionDeStock.API.Dtos
{
    public class RecordCountDto
    {
        public decimal CountedQuantity { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}