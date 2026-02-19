using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class novapermissaocaixa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 83, "Cadastros", 83, null, "visualizar conta bancária" },
                    { 84, "Cadastros", 84, null, "Criar conta bancária" },
                    { 85, "Cadastros", 85, null, "editar conta bancária" },
                    { 86, "Financeiro", 86, null, "Ver caixa" },
                    { 87, "Financeiro", 87, null, "Abrir caixa" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 87);
        }
    }
}
