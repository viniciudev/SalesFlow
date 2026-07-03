using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class matrizfiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_purchaseItem_tb_product_ProdutoId",
                table: "tb_purchaseItem");

            migrationBuilder.AddColumn<int>(
                name: "SituacaoTributariaId",
                table: "tb_product",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_situacaoTributaria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_situacaoTributaria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_situacaoTributaria_tb_company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_regraFiscal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NaturezaOperacaoId = table.Column<int>(type: "integer", nullable: false),
                    SituacaoTributariaId = table.Column<int>(type: "integer", nullable: false),
                    Destino = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Cfop = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RF_AplicarICMS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_CstICMS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_CsosnICMS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_AliquotaICMS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_ReduzirBaseICMS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_AplicarIPI = table.Column<bool>(type: "boolean", nullable: true),
                    RF_CstIPI = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_AliquotaIPI = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_AplicarPIS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_CstPIS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_AliquotaPIS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_AplicarCOFINS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_CstCOFINS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_AliquotaCOFINS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_AplicarISSQN = table.Column<bool>(type: "boolean", nullable: true),
                    RF_AliquotaISSQN = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_AplicarIBS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_CstIBS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_AliquotaIBS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_AplicarCBS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_CstCBS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RF_AliquotaCBS = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    RF_AplicarIS = table.Column<bool>(type: "boolean", nullable: true),
                    RF_AliquotaIS = table.Column<decimal>(type: "numeric(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_regraFiscal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_regraFiscal_tb_naturezaOperacao_NaturezaOperacaoId",
                        column: x => x.NaturezaOperacaoId,
                        principalTable: "tb_naturezaOperacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_regraFiscal_tb_situacaoTributaria_SituacaoTributariaId",
                        column: x => x.SituacaoTributariaId,
                        principalTable: "tb_situacaoTributaria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_product_SituacaoTributariaId",
                table: "tb_product",
                column: "SituacaoTributariaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_regraFiscal_NaturezaOperacaoId_SituacaoTributariaId_Dest~",
                table: "tb_regraFiscal",
                columns: new[] { "NaturezaOperacaoId", "SituacaoTributariaId", "Destino" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_regraFiscal_SituacaoTributariaId",
                table: "tb_regraFiscal",
                column: "SituacaoTributariaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_situacaoTributaria_CompanyId_Codigo",
                table: "tb_situacaoTributaria",
                columns: new[] { "CompanyId", "Codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_product_tb_situacaoTributaria_SituacaoTributariaId",
                table: "tb_product",
                column: "SituacaoTributariaId",
                principalTable: "tb_situacaoTributaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_purchaseItem_tb_product_ProdutoId",
                table: "tb_purchaseItem",
                column: "ProdutoId",
                principalTable: "tb_product",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_product_tb_situacaoTributaria_SituacaoTributariaId",
                table: "tb_product");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_purchaseItem_tb_product_ProdutoId",
                table: "tb_purchaseItem");

            migrationBuilder.DropTable(
                name: "tb_regraFiscal");

            migrationBuilder.DropTable(
                name: "tb_situacaoTributaria");

            migrationBuilder.DropIndex(
                name: "IX_tb_product_SituacaoTributariaId",
                table: "tb_product");

            migrationBuilder.DropColumn(
                name: "SituacaoTributariaId",
                table: "tb_product");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_purchaseItem_tb_product_ProdutoId",
                table: "tb_purchaseItem",
                column: "ProdutoId",
                principalTable: "tb_product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
