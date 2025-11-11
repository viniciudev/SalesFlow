using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class alterstockcolunasidcompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCompany",
                table: "tb_stock",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_tb_stock_IdCompany",
                table: "tb_stock",
                column: "IdCompany");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_stock_tb_company_IdCompany",
                table: "tb_stock",
                column: "IdCompany",
                principalTable: "tb_company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_stock_tb_company_IdCompany",
                table: "tb_stock");

            migrationBuilder.DropIndex(
                name: "IX_tb_stock_IdCompany",
                table: "tb_stock");

            migrationBuilder.DropColumn(
                name: "IdCompany",
                table: "tb_stock");
        }
    }
}
