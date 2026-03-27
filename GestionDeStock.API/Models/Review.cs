using GestionDeStock.API.Interfaces;

namespace GestionDeStock.API.Models
{
public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }       // produit_id
    public Product? Product { get; set; }
    public string Author { get; set; } = "";  // auteur
    public string Comment { get; set; } = ""; // commentaire
    public float Rating { get; set; }         // note
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Alias PHP dans la sérialisation (via JsonPropertyName)
    // géré directement dans FormatProduct
}
}