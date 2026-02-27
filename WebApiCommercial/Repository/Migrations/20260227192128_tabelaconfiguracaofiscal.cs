using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tabelaconfiguracaofiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_fiscalConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nfe_Serie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nfe_NumeroInicial = table.Column<long>(type: "bigint", nullable: false),
                    Nfce_Serie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nfce_NumeroInicial = table.Column<long>(type: "bigint", nullable: false),
                    Certificado_Arquivo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Certificado_Senha = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Csc_Identificador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Csc_Valor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ambiente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Emitente_Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Emitente_Cpf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Emitente_InscricaoEstadual = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Emitente_RazaoSocial = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Emitente_Fantasia = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Emitente_Telefone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Emitente_Cep = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Emitente_Logradouro = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Emitente_Numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Emitente_Complemento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Emitente_Bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Emitente_CodigoCidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Emitente_Cidade = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Emitente_Uf = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Emitente_Crt = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AutorizacaoASO = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_fiscalConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_fiscalConfiguration_tb_company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_fiscalConfiguration_CompanyId",
                table: "tb_fiscalConfiguration",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_fiscalConfiguration");
        }
    }
}
