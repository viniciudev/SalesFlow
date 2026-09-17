using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class addfinanceiroordemservico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdServiceOrder",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdServiceOrder",
                table: "tb_financial",
                column: "IdServiceOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_serviceOrder_IdServiceOrder",
                table: "tb_financial",
                column: "IdServiceOrder",
                principalTable: "tb_serviceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_serviceOrder_IdServiceOrder",
                table: "tb_financial");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_IdServiceOrder",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "IdServiceOrder",
                table: "tb_financial");
        }
    }
}
