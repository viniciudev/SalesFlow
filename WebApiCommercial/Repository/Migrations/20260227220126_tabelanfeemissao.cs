using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tabelanfeemissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_nfeEmission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NaturezaOperacaoId = table.Column<int>(type: "integer", nullable: false),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    TipoDocumento = table.Column<int>(type: "integer", nullable: false),
                    Serie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Numero = table.Column<long>(type: "bigint", nullable: true),
                    Sent = table.Column<bool>(type: "boolean", nullable: false),
                    TryCount = table.Column<int>(type: "integer", nullable: false),
                    RequestPayloadJson = table.Column<string>(type: "text", nullable: true),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComapanyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_nfeEmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_nfeEmission_tb_company_ComapanyId",
                        column: x => x.ComapanyId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_nfeEmission_tb_naturezaOperacao_NaturezaOperacaoId",
                        column: x => x.NaturezaOperacaoId,
                        principalTable: "tb_naturezaOperacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_nfeEmission_tb_sale_SaleId",
                        column: x => x.SaleId,
                        principalTable: "tb_sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_nfeEmission_ComapanyId",
                table: "tb_nfeEmission",
                column: "ComapanyId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_nfeEmission_NaturezaOperacaoId",
                table: "tb_nfeEmission",
                column: "NaturezaOperacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_nfeEmission_SaleId",
                table: "tb_nfeEmission",
                column: "SaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_nfeEmission");
        }
    }
}
