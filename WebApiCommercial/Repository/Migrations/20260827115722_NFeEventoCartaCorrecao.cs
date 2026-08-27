using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Cria a tabela tb_nfeEvento para persistir eventos fiscais vinculados à NF-e
    /// (ex.: Carta de Correção Eletrônica - CC-e, tpEvento 110110).
    ///
    /// OBSERVAÇÃO: o scaffold do EF gerou dezenas de AlterColumn espúrios em tabelas
    /// de outros módulos (drift pré-existente entre o snapshot - timestamp with time
    /// zone - e o modelo atual - timestamp without time zone). Eles foram REMOVIDOS
    /// desta migração para não alterar colunas fora do escopo da CC-e. Pendência
    /// registrada para o time decidir o alinhamento do tipo de data no banco.
    /// </remarks>
    public partial class NFeEventoCartaCorrecao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_nfeEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NFeEmissionId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    TipoEvento = table.Column<int>(type: "integer", nullable: false),
                    DescricaoEvento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NSeqEvento = table.Column<int>(type: "integer", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    Correcao = table.Column<string>(type: "text", nullable: true),
                    Protocolo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    CStat = table.Column<int>(type: "integer", nullable: true),
                    XMotivo = table.Column<string>(type: "text", nullable: true),
                    DhRegEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    XmlEvento = table.Column<string>(type: "text", nullable: true),
                    Situacao = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_nfeEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_nfeEvento_tb_company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_nfeEvento_tb_nfeEmission_NFeEmissionId",
                        column: x => x.NFeEmissionId,
                        principalTable: "tb_nfeEmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_nfeEvento_CompanyId",
                table: "tb_nfeEvento",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_nfeEvento_NFeEmissionId_TipoEvento_Situacao",
                table: "tb_nfeEvento",
                columns: new[] { "NFeEmissionId", "TipoEvento", "Situacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_nfeEvento");
        }
    }
}
