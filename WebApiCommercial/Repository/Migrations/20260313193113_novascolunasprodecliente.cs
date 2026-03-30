using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class novascolunasprodecliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ncm",
                table: "tb_product",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CodigoMunicipio",
                table: "tb_client",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "tb_client",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ncm",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "CodigoMunicipio",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "tb_client");
        }
    }
}
