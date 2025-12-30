using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tablebox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoxId",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_box",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValorInicial = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorFinal = table.Column<decimal>(type: "numeric", nullable: true),
                    SaldoCalculado = table.Column<decimal>(type: "numeric", nullable: true),
                    Diferenca = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_box", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_box_tb_company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tb_company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_box_CompanyId",
                table: "tb_box",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_box_IdProduct",
                table: "tb_financial",
                column: "IdProduct",
                principalTable: "tb_box",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_box_IdProduct",
                table: "tb_financial");

            migrationBuilder.DropTable(
                name: "tb_box");

            migrationBuilder.DropColumn(
                name: "BoxId",
                table: "tb_financial");
        }
    }
}
