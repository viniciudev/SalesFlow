using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class colunatrocofin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_box_IdProduct",
                table: "tb_financial");

            migrationBuilder.AddColumn<decimal>(
                name: "Troco",
                table: "tb_financial",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_BoxId",
                table: "tb_financial",
                column: "BoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_box_BoxId",
                table: "tb_financial",
                column: "BoxId",
                principalTable: "tb_box",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_box_BoxId",
                table: "tb_financial");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_BoxId",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "Troco",
                table: "tb_financial");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_box_IdProduct",
                table: "tb_financial",
                column: "IdProduct",
                principalTable: "tb_box",
                principalColumn: "Id");
        }
    }
}
