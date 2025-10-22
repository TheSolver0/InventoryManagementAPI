using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeStock.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeAmountDecimalInMoveTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Movements",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "Movements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
