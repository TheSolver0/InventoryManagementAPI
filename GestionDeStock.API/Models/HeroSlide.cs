public class HeroSlide
{
    public int Id { get; set; }
    public string BadgeTexte { get; set; } = "";
    public string Titre { get; set; } = "";
    public string SousTitre { get; set; } = "";
    public decimal Prix { get; set; }
    public decimal? AncienPrix { get; set; }
    public string Image { get; set; } = "";
    public int StockPct { get; set; } = 50;
    public string BtnPrincipalLien { get; set; } = "catalog.php";
    public bool IsActive { get; set; } = true;
    public int Ordre { get; set; } = 0;
}