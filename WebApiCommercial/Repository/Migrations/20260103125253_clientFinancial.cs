using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class clientFinancial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdClient",
                table: "tb_financial",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_ClientId",
                table: "tb_financial",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_client_ClientId",
                table: "tb_financial",
                column: "ClientId",
                principalTable: "tb_client",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_client_ClientId",
                table: "tb_financial");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_ClientId",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "IdClient",
                table: "tb_financial");
        }
    }
}
