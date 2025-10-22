using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeStock.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnSeuilToThresholdTableProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Seuil",
                table: "Products",
                newName: "Threshold");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Threshold",
                table: "Products",
                newName: "Seuil");
        }
    }
}
