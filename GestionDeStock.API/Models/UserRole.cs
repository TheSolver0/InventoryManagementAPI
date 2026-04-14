namespace GestionDeStock.API.Models
{
    public enum UserRole
    {
        Admin = 1,    // Accès total + gestion des utilisateurs
        Gerant = 2,   // Accès total (sauf gestion des utilisateurs)
        Employe = 3   // Accès opérationnel uniquement (produits, catégories, inventaire, commandes)
    }
}
