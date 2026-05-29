using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tabelasparacompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_provider",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    razaoSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    nomeFantasia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    inscricaoEstadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    cep = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    complemento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    idcnae = table.Column<int>(type: "integer", nullable: false),
                    nomecnae = table.Column<string>(type: "text", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_provider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_provider_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCompra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChaveNfe = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_purchase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_purchase_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_purchase_tb_provider_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "tb_provider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_purchaseItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompraId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: true),
                    CodigoProduto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DescricaoProduto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Desconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_purchaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_purchaseItem_tb_product_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "tb_product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_purchaseItem_tb_purchase_CompraId",
                        column: x => x.CompraId,
                        principalTable: "tb_purchase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 100, "Compras", 100, null, "Visualizar Compras" },
                    { 101, "Compras", 101, null, "Criar Compras" },
                    { 102, "Compras", 102, null, "Cancelar Compras" },
                    { 103, "Compras", 103, null, "Deletar Compras" },
                    { 104, "Compras", 104, null, "Editar Compras" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_provider_IdCompany",
                table: "tb_provider",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_purchase_FornecedorId",
                table: "tb_purchase",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_purchase_IdCompany",
                table: "tb_purchase",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_purchaseItem_CompraId",
                table: "tb_purchaseItem",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_purchaseItem_ProdutoId",
                table: "tb_purchaseItem",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_purchaseItem");

            migrationBuilder.DropTable(
                name: "tb_purchase");

            migrationBuilder.DropTable(
                name: "tb_provider");

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 104);
        }
    }
}
