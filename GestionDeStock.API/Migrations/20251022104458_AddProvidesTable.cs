using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProvidesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Provides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Provides_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Provides_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Provides_ProductId",
                table: "Provides",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Provides_SupplierId",
                table: "Provides",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Provides");
        }
    }
}
