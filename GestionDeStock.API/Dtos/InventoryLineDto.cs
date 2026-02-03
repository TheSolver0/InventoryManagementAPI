namespace GestionDeStock.API.Dtos
{
    public class InventoryLineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductSku { get; set; }
        public string? ProductName { get; set; }
        public string? Location { get; set; }
        public decimal TheoreticalQuantity { get; set; }
        public decimal? CountedQuantity { get; set; }
        public decimal Variance { get; set; }
        public DateTime? CountedAt { get; set; }
        public string? CountedBy { get; set; }
        public string? Notes { get; set; }
    }
}