using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class correcaoeaddtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialPaymentMethod_tb_financial_FinancialId",
                table: "FinancialPaymentMethod");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialPaymentMethod_tb_paymentMethod_PaymentMethodId",
                table: "FinancialPaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_tb_naturezaOperacao_Cfop_TipoDocumento",
                table: "tb_naturezaOperacao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinancialPaymentMethod",
                table: "FinancialPaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_FinancialPaymentMethod_FinancialId",
                table: "FinancialPaymentMethod");

            migrationBuilder.RenameTable(
                name: "FinancialPaymentMethod",
                newName: "tb_financialPaymentMethod");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialPaymentMethod_PaymentMethodId",
                table: "tb_financialPaymentMethod",
                newName: "IX_tb_financialPaymentMethod_PaymentMethodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_financialPaymentMethod",
                table: "tb_financialPaymentMethod",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financialPaymentMethod_FinancialId_PaymentMethodId",
                table: "tb_financialPaymentMethod",
                columns: new[] { "FinancialId", "PaymentMethodId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financialPaymentMethod_tb_financial_FinancialId",
                table: "tb_financialPaymentMethod",
                column: "FinancialId",
                principalTable: "tb_financial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financialPaymentMethod_tb_paymentMethod_PaymentMethodId",
                table: "tb_financialPaymentMethod",
                column: "PaymentMethodId",
                principalTable: "tb_paymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financialPaymentMethod_tb_financial_FinancialId",
                table: "tb_financialPaymentMethod");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_financialPaymentMethod_tb_paymentMethod_PaymentMethodId",
                table: "tb_financialPaymentMethod");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_financialPaymentMethod",
                table: "tb_financialPaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_tb_financialPaymentMethod_FinancialId_PaymentMethodId",
                table: "tb_financialPaymentMethod");

            migrationBuilder.RenameTable(
                name: "tb_financialPaymentMethod",
                newName: "FinancialPaymentMethod");

            migrationBuilder.RenameIndex(
                name: "IX_tb_financialPaymentMethod_PaymentMethodId",
                table: "FinancialPaymentMethod",
                newName: "IX_FinancialPaymentMethod_PaymentMethodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinancialPaymentMethod",
                table: "FinancialPaymentMethod",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_naturezaOperacao_Cfop_TipoDocumento",
                table: "tb_naturezaOperacao",
                columns: new[] { "Cfop", "TipoDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPaymentMethod_FinancialId",
                table: "FinancialPaymentMethod",
                column: "FinancialId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialPaymentMethod_tb_financial_FinancialId",
                table: "FinancialPaymentMethod",
                column: "FinancialId",
                principalTable: "tb_financial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialPaymentMethod_tb_paymentMethod_PaymentMethodId",
                table: "FinancialPaymentMethod",
                column: "PaymentMethodId",
                principalTable: "tb_paymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
