using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Novatabelanaturezaoperacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_naturezaOperacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cfop = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TipoDocumento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Finalidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConsumidorFinal = table.Column<bool>(type: "boolean", nullable: false),
                    MovimentaEstoque = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    AplicarICMS = table.Column<bool>(type: "boolean", nullable: true),
                    CstICMS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliquotaICMS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ReduzirBaseICMS = table.Column<bool>(type: "boolean", nullable: true),
                    AplicarIPI = table.Column<bool>(type: "boolean", nullable: true),
                    CstIPI = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliquotaIPI = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    AplicarPIS = table.Column<bool>(type: "boolean", nullable: true),
                    CstPIS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliquotaPIS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    AplicarCOFINS = table.Column<bool>(type: "boolean", nullable: true),
                    CstCOFINS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliquotaCOFINS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    AplicarISSQN = table.Column<bool>(type: "boolean", nullable: true),
                    AliquotaISSQN = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    AplicarIBS = table.Column<bool>(type: "boolean", nullable: true),
                    CstIBS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliquotaIBS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    AplicarCBS = table.Column<bool>(type: "boolean", nullable: true),
                    CstCBS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliquotaCBS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    AplicarIS = table.Column<bool>(type: "boolean", nullable: true),
                    AliquotaIS = table.Column<decimal>(type: "numeric(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_naturezaOperacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_naturezaOperacao_tb_company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_naturezaOperacao_Cfop_TipoDocumento",
                table: "tb_naturezaOperacao",
                columns: new[] { "Cfop", "TipoDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_naturezaOperacao_CompanyId",
                table: "tb_naturezaOperacao",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_naturezaOperacao");
        }
    }
}
