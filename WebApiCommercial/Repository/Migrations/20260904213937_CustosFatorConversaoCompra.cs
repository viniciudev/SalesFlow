using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class CustosFatorConversaoCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FatorConversao",
                table: "tb_purchaseItem",
                type: "numeric(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeXml",
                table: "tb_purchaseItem",
                type: "numeric(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unidade",
                table: "tb_purchaseItem",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorUnitarioXml",
                table: "tb_purchaseItem",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCalculoIBSCBS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCalculoICMS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustosExtrasJson",
                table: "tb_purchase",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacao",
                table: "tb_purchase",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorCBS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorCOFINS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDesconto",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorFrete",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorIBS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorICMS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorIPI",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorNotaFiscal",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPIS",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorProdutos",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorSeguro",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotalTributos",
                table: "tb_purchase",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoUnitario",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FatorConversao",
                table: "tb_purchaseItem");

            migrationBuilder.DropColumn(
                name: "QuantidadeXml",
                table: "tb_purchaseItem");

            migrationBuilder.DropColumn(
                name: "Unidade",
                table: "tb_purchaseItem");

            migrationBuilder.DropColumn(
                name: "ValorUnitarioXml",
                table: "tb_purchaseItem");

            migrationBuilder.DropColumn(
                name: "BaseCalculoIBSCBS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "BaseCalculoICMS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "CustosExtrasJson",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "Observacao",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorCBS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorCOFINS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorDesconto",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorFrete",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorIBS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorICMS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorIPI",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorNotaFiscal",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorPIS",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorProdutos",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorSeguro",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "ValorTotalTributos",
                table: "tb_purchase");

            migrationBuilder.DropColumn(
                name: "PesoUnitario",
                table: "tb_product");
        }
    }
}
