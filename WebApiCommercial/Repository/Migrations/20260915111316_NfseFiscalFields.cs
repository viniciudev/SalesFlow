using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class NfseFiscalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Dps_NumeroInicial",
                table: "tb_fiscalConfiguration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Dps_Serie",
                table: "tb_fiscalConfiguration",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Emitente_InscricaoMunicipal",
                table: "tb_fiscalConfiguration",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Emitente_OpcaoSimplesNacional",
                table: "tb_fiscalConfiguration",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dps_NumeroInicial",
                table: "tb_fiscalConfiguration");

            migrationBuilder.DropColumn(
                name: "Dps_Serie",
                table: "tb_fiscalConfiguration");

            migrationBuilder.DropColumn(
                name: "Emitente_InscricaoMunicipal",
                table: "tb_fiscalConfiguration");

            migrationBuilder.DropColumn(
                name: "Emitente_OpcaoSimplesNacional",
                table: "tb_fiscalConfiguration");
        }
    }
}
