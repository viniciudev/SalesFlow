using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tabelaligafinaometodopagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "FinancialPaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FinancialId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Installments = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialPaymentMethod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialPaymentMethod_tb_financial_FinancialId",
                        column: x => x.FinancialId,
                        principalTable: "tb_financial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialPaymentMethod_tb_paymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "tb_paymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPaymentMethod_FinancialId",
                table: "FinancialPaymentMethod",
                column: "FinancialId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPaymentMethod_PaymentMethodId",
                table: "FinancialPaymentMethod",
                column: "PaymentMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialPaymentMethod");

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
    }
}
