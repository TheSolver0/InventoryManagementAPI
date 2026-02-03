namespace GestionDeStock.API.Models
{
    public static class StockMovementTypes
    {
        public const string Reception = "Réception";        // Réception fournisseur
        public const string Expedition = "Expédition";      // Expédition client
        public const string Adjustment = "Ajustement";      // Ajustement inventaire
        public const string Transfer = "Transfert";         // Transfert entre entrepôts
        public const string Return = "Retour";              // Retour client ou fournisseur
        public const string Loss = "Perte";                 // Perte, vol, casse
        public const string Production = "Production";      // Fabrication (si applicable)
        public const string Consumption = "Consommation";   // Consommation matière première
    }
}