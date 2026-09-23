using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class PartnerProfilesDriverLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue: 1 = PartnerProfile.Cliente. Todo cadastro que já
            // existe no sistema é, no mínimo, um cliente — usar 0 (None) aqui
            // deixaria os registros anteriores sem nenhuma classificação.
            migrationBuilder.AddColumn<int>(
                name: "Profiles",
                table: "tb_client",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "tb_driver_license",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdClient = table.Column<int>(type: "integer", nullable: false),
                    NumeroCnh = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    CategoriaCnh = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DataEmissaoCnh = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataValidadeCnh = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PrimeiraHabilitacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UfEmissaoCnh = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    PossuiEar = table.Column<bool>(type: "boolean", nullable: false),
                    ObservacoesCnh = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_driver_license", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_driver_license_tb_client_IdClient",
                        column: x => x.IdClient,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_driver_license_IdClient",
                table: "tb_driver_license",
                column: "IdClient",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_driver_license");

          
            migrationBuilder.DropColumn(
                name: "Profiles",
                table: "tb_client");

        }
    }
}
