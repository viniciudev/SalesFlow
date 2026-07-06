using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Novapermissaosituacaotrib : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RF_cClassTrib",
                table: "tb_regraFiscal",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prod_cClassTrib",
                table: "tb_product",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cClassTrib",
                table: "tb_naturezaOperacao",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 113, "Situação Tributária", 113, null, "Visualizar Situação Tributária" },
                    { 114, "Situação Tributária", 114, null, "Criar Situação Tributária" },
                    { 115, "Situação Tributária", 115, null, "Editar Situação Tributária" },
                    { 116, "Situação Tributária", 116, null, "Deletar Situação Tributária" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DropColumn(
                name: "RF_cClassTrib",
                table: "tb_regraFiscal");

            migrationBuilder.DropColumn(
                name: "Prod_cClassTrib",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "cClassTrib",
                table: "tb_naturezaOperacao");
        }
    }
}
