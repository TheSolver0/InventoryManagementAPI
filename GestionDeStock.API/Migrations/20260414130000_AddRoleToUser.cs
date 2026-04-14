using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1); // 1 = Admin — les comptes existants deviennent Admin

            // Les nouveaux comptes créés via /api/auth/register auront le rôle Employe (3)
            // défini dans le code ; la valeur par défaut DB (1) ne s'applique qu'aux lignes
            // existantes sans valeur fournie (ex : migration sur une base déjà peuplée).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
