using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class novacolunanofinanceirocontabanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_BankAccountId",
                table: "tb_financial",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_bankAccount_BankAccountId",
                table: "tb_financial",
                column: "BankAccountId",
                principalTable: "tb_bankAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_bankAccount_BankAccountId",
                table: "tb_financial");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_BankAccountId",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "tb_financial");
        }
    }
}
