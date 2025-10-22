using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionDeStock.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeSupplierIdNullableOnMovementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Customers_CustomerId",
                table: "Movements");

            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Suppliers_SupplierId",
                table: "Movements");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Movements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Movements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Customers_CustomerId",
                table: "Movements",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Suppliers_SupplierId",
                table: "Movements",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Customers_CustomerId",
                table: "Movements");

            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Suppliers_SupplierId",
                table: "Movements");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Movements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Movements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Customers_CustomerId",
                table: "Movements",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Suppliers_SupplierId",
                table: "Movements",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
