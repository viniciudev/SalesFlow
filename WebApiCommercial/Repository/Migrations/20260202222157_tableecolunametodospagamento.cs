using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tableecolunametodospagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "tb_financial");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_PaymentMethodId",
                table: "tb_financial",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_paymentMethod_PaymentMethodId",
                table: "tb_financial",
                column: "PaymentMethodId",
                principalTable: "tb_paymentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_paymentMethod_PaymentMethodId",
                table: "tb_financial");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_PaymentMethodId",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "tb_financial");

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "tb_financial",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
