using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class idpurchasenacompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPurchase",
                table: "tb_financial",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_PurchaseId",
                table: "tb_financial",
                column: "PurchaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_purchase_PurchaseId",
                table: "tb_financial",
                column: "PurchaseId",
                principalTable: "tb_purchase",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_purchase_PurchaseId",
                table: "tb_financial");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_PurchaseId",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "IdPurchase",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "tb_financial");
        }
    }
}
