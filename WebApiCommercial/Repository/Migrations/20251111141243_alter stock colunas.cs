using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class alterstockcolunas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "tb_stock");

            migrationBuilder.RenameColumn(
                name: "MyProperty",
                table: "tb_stock",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Movement",
                table: "tb_stock",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "tb_stock",
                newName: "Reason");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "tb_stock",
                newName: "MyProperty");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "tb_stock",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "tb_stock",
                newName: "Movement");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "tb_stock",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
