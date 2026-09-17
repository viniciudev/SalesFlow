using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class addpermissionnotaservico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 121, "Nota de serviço", 121, null, "Visualizar nota de serviço" },
                    { 122, "Nota de serviço", 122, null, "Criar nota de serviço" },
                    { 123, "Nota de serviço", 123, null, "Editar nota de serviço" },
                    { 124, "Nota de serviço", 124, null, "Deletar nota de serviço" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 124);
        }
    }
}
