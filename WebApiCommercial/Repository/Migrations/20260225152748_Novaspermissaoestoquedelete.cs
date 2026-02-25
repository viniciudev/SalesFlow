using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Novaspermissaoestoquedelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "tb_permission",
            //    keyColumn: "Id",
            //    keyValue: 89);

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    //{ 93, "Estoque", 93, null, "Criar Estoque" },
                    { 94, "Estoque", 94, null, "Deletar Estoque" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "tb_permission",
            //    keyColumn: "Id",
            //    keyValue: 93);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 94);

            //migrationBuilder.InsertData(
            //    table: "tb_permission",
            //    columns: new[] { "Id", "Category", "Code", "Description", "Name" },
            //    values: new object[] { 89, "Estoque", 89, null, "Criar Estoque" });
        }
    }
}
