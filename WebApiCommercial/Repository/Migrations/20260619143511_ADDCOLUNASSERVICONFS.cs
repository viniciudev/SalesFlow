using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
	/// <inheritdoc />
	public partial class ADDCOLUNASSERVICONFS : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// ===== 1º PASSO: ALTERAR A COLUNA PARA ACEITAR NULL =====
			migrationBuilder.AlterColumn<int>(
					name: "IdPurchase",
					table: "tb_financial",
					type: "integer",
					nullable: true,
					oldClrType: typeof(int),
					oldType: "integer");

			// ===== 2º PASSO: CORRIGIR OS DADOS (agora pode ser NULL) =====
			migrationBuilder.Sql(
					@"UPDATE tb_financial 
           SET ""IdPurchase"" = NULL 
           WHERE ""IdPurchase"" = 0;"
			);

			migrationBuilder.Sql(
					@"UPDATE tb_financial 
           SET ""IdPurchase"" = NULL 
           WHERE ""IdPurchase"" IS NOT NULL 
           AND ""IdPurchase"" NOT IN (SELECT ""Id"" FROM tb_purchase);"
			);

			migrationBuilder.DropForeignKey(
								name: "FK_tb_financial_tb_purchase_PurchaseId",
								table: "tb_financial");

			migrationBuilder.DropIndex(
					name: "IX_tb_financial_PurchaseId",
					table: "tb_financial");

			migrationBuilder.DropColumn(
					name: "PurchaseId",
					table: "tb_financial");

			migrationBuilder.RenameColumn(
					name: "Experience",
					table: "tb_ServiceProvided",
					newName: "ServiceMode");

			migrationBuilder.RenameColumn(
					name: "Deadline",
					table: "tb_ServiceProvided",
					newName: "ServiceLink");

			migrationBuilder.RenameColumn(
					name: "Capacity",
					table: "tb_ServiceProvided",
					newName: "PropertyRegistry");

			migrationBuilder.AddColumn<string>(
					name: "CibCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "ConstructionCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "CurrencyCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "Description",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<DateTime>(
					name: "EventEndDate",
					table: "tb_ServiceProvided",
					type: "timestamp with time zone",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "EventIdentifier",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "EventName",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<DateTime>(
					name: "EventStartDate",
					table: "tb_ServiceProvided",
					type: "timestamp with time zone",
					nullable: true);

			migrationBuilder.AddColumn<decimal>(
					name: "ForeignValue",
					table: "tb_ServiceProvided",
					type: "numeric",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "InternalContributorCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "LocationCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "MunicipalTaxCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "NationalTaxCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "NbsCode",
					table: "tb_ServiceProvided",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<int>(
					name: "SpecialType",
					table: "tb_ServiceProvided",
					type: "integer",
					nullable: true);

			migrationBuilder.AlterColumn<int>(
					name: "IdPurchase",
					table: "tb_financial",
					type: "integer",
					nullable: true,
					oldClrType: typeof(int),
					oldType: "integer");

			migrationBuilder.InsertData(
					table: "tb_permission",
					columns: new[] { "Id", "Category", "Code", "Description", "Name" },
					values: new object[,]
					{
										{ 109, "Serviços", 109, null, "Visualizar Serviço Prestado" },
										{ 110, "Serviços", 110, null, "Criar Serviço Prestado" },
										{ 111, "Serviços", 111, null, "Editar Serviço Prestado" },
										{ 112, "Serviços", 112, null, "Deletar Serviço Prestado" }
					});

			migrationBuilder.CreateIndex(
					name: "IX_tb_financial_IdPurchase",
					table: "tb_financial",
					column: "IdPurchase");

			migrationBuilder.AddForeignKey(
					name: "FK_tb_financial_tb_purchase_IdPurchase",
					table: "tb_financial",
					column: "IdPurchase",
					principalTable: "tb_purchase",
					principalColumn: "Id",
					onDelete: ReferentialAction.Restrict);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_tb_financial_tb_purchase_IdPurchase",
					table: "tb_financial");

			migrationBuilder.DropIndex(
					name: "IX_tb_financial_IdPurchase",
					table: "tb_financial");

			migrationBuilder.DeleteData(
					table: "tb_permission",
					keyColumn: "Id",
					keyValue: 109);

			migrationBuilder.DeleteData(
					table: "tb_permission",
					keyColumn: "Id",
					keyValue: 110);

			migrationBuilder.DeleteData(
					table: "tb_permission",
					keyColumn: "Id",
					keyValue: 111);

			migrationBuilder.DeleteData(
					table: "tb_permission",
					keyColumn: "Id",
					keyValue: 112);

			migrationBuilder.DropColumn(
					name: "CibCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "ConstructionCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "CurrencyCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "Description",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "EventEndDate",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "EventIdentifier",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "EventName",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "EventStartDate",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "ForeignValue",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "InternalContributorCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "LocationCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "MunicipalTaxCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "NationalTaxCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "NbsCode",
					table: "tb_ServiceProvided");

			migrationBuilder.DropColumn(
					name: "SpecialType",
					table: "tb_ServiceProvided");

			migrationBuilder.RenameColumn(
					name: "ServiceMode",
					table: "tb_ServiceProvided",
					newName: "Experience");

			migrationBuilder.RenameColumn(
					name: "ServiceLink",
					table: "tb_ServiceProvided",
					newName: "Deadline");

			migrationBuilder.RenameColumn(
					name: "PropertyRegistry",
					table: "tb_ServiceProvided",
					newName: "Capacity");

			migrationBuilder.AlterColumn<int>(
					name: "IdPurchase",
					table: "tb_financial",
					type: "integer",
					nullable: false,
					defaultValue: 0,
					oldClrType: typeof(int),
					oldType: "integer",
					oldNullable: true);

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
	}
}
