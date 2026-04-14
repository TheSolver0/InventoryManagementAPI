namespace GestionDeStock.API.Auth
{
    /// <summary>
    /// Constantes de rôles utilisées dans les attributs [Authorize(Roles = ...)].
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Gerant = "Gerant";
        public const string Employe = "Employe";

        /// <summary>Admin ou Gérant (accès aux données financières et à la gestion)</summary>
        public const string AdminOrGerant = "Admin,Gerant";
    }
}
