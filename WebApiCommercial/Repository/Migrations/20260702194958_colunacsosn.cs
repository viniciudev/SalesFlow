using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class colunacsosn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfiguracaoTributaria_CsosnICMS",
                table: "tb_product",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfiguracaoTributaria_CsosnICMS",
                table: "tb_naturezaOperacao",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfiguracaoTributaria_CsosnICMS",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "ConfiguracaoTributaria_CsosnICMS",
                table: "tb_naturezaOperacao");
        }
    }
}
