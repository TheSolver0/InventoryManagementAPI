using GestionDeStock.API.Models;
namespace GestionDeStock.API.Dtos
{
public class ReviewDto
{
    public required int    ProductId { get; set; }
    public required string Author    { get; set; }
    public required float  Rating    { get; set; }
    public required string Comment   { get; set; }
}
}