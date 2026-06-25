using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tbordemetributacaoitens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacaoTributaria",
                table: "tb_product",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NaturezaOperacaoOrigemId",
                table: "tb_product",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaCBS",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaCOFINS",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaIBS",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaICMS",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaIPI",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaIS",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaISSQN",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Prod_AliquotaPIS",
                table: "tb_product",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarCBS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarCOFINS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarIBS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarICMS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarIPI",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarIS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarISSQN",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_AplicarPIS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_CstCBS",
                table: "tb_product",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_CstCOFINS",
                table: "tb_product",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_CstIBS",
                table: "tb_product",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_CstICMS",
                table: "tb_product",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_CstIPI",
                table: "tb_product",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_CstPIS",
                table: "tb_product",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prod_ReduzirBaseICMS",
                table: "tb_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsaTributacaoPropria",
                table: "tb_product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TributacaoAuditJson",
                table: "tb_nfeEmission",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteTributacaoPorProduto",
                table: "tb_naturezaOperacao",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CodMunIBGE",
                table: "tb_fiscalConfiguration",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LastInvoiceNumber",
                table: "tb_fiscalConfiguration",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "tb_company",
                keyColumn: "Id",
                keyValue: 1,
                column: "CorporateName",
                value: "Empresa Padr�o");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataAtualizacaoTributaria",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "NaturezaOperacaoOrigemId",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaCBS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaCOFINS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaIBS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaICMS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaIPI",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaIS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaISSQN",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AliquotaPIS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarCBS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarCOFINS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarIBS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarICMS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarIPI",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarIS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarISSQN",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_AplicarPIS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_CstCBS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_CstCOFINS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_CstIBS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_CstICMS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_CstIPI",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_CstPIS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "Prod_ReduzirBaseICMS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "UsaTributacaoPropria",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "TributacaoAuditJson",
                table: "tb_nfeEmission");

            migrationBuilder.DropColumn(
                name: "PermiteTributacaoPorProduto",
                table: "tb_naturezaOperacao");

            migrationBuilder.DropColumn(
                name: "CodMunIBGE",
                table: "tb_fiscalConfiguration");

            migrationBuilder.DropColumn(
                name: "LastInvoiceNumber",
                table: "tb_fiscalConfiguration");

            migrationBuilder.UpdateData(
                table: "tb_company",
                keyColumn: "Id",
                keyValue: 1,
                column: "CorporateName",
                value: "Empresa Padrão");
        }
    }
}
