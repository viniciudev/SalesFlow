using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <summary>
    /// MDF-e (modelo 58) — fase 1: emissão, numeração e o RNTRC do emitente.
    ///
    /// <b>Esta migration foi revisada à mão.</b> O scaffold do EF gerou, além do
    /// que está aqui, 3 <c>AlterColumn</c> de data em <c>tb_driver_license</c>
    /// (<c>DataEmissaoCnh</c>, <c>DataValidadeCnh</c>, <c>PrimeiraHabilitacao</c>).
    /// Foram REMOVIDOS — ver a decisão abaixo.
    ///
    /// Aquelas 3 colunas são as únicas do banco que são
    /// <c>timestamp without time zone</c>; as outras 44 colunas de data são
    /// <c>timestamp with time zone</c>. O snapshot registra corretamente o que
    /// existe no banco, mas o modelo de design time mapeia <c>DateTime</c> para
    /// timestamptz, então o diff propõe CONVERTER essas 3 colunas — mexendo em
    /// dados de CNH de outro módulo, que não têm nada a ver com o MDF-e.
    ///
    /// <b>Decisão:</b> não converter, e não declarar
    /// <c>HasColumnType("timestamp without time zone")</c> em <c>ConfiguraDriverLicense</c>
    /// agora. A declaração não é inócua como parece: hoje o Npgsql envia o
    /// parâmetro como timestamptz e o Postgres converte para a coluna usando o
    /// TimeZone da sessão; declarando o store type, o Npgsql passa a enviar
    /// <c>timestamp without time zone</c> e o valor gravado deixa de sofrer essa
    /// conversão. Ou seja, mudaria a hora efetivamente gravada nas datas de CNH
    /// quando a sessão não estiver em UTC — o oposto de "só alinhar o modelo ao
    /// banco". Zerar o drift é uma mudança própria, com dado real em jogo, e não
    /// deve pegar carona numa entrega de MDF-e. Consequência aceita: os 3
    /// <c>AlterColumn</c> vão reaparecer no próximo <c>migrations add</c>.
    ///
    /// Conteúdo real: 5 tabelas novas, 3 colunas novas em
    /// <c>tb_fiscalConfiguration</c> (série, número inicial e RNTRC do
    /// emitente), 20 índices e o seed das 4 permissões.
    /// </summary>
    public partial class AddMdfe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RM09 — série e número inicial do MDF-e, na configuração fiscal,
            // como o DPS já faz. O emitente é quem decide a numeração, e ela
            // precisa ser independente da numeração de NF-e.
            migrationBuilder.AddColumn<string>(
                name: "Mdfe_Serie",
                table: "tb_fiscalConfiguration",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Mdfe_NumeroInicial",
                table: "tb_fiscalConfiguration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // RM08 — o MDF-e de Prestador de Serviço de Transporte (PST) exige
            // o RNTRC do emitente. 8 caracteres é o tipo TRNTRC dos XSDs
            // ([0-9]{8}), o mesmo limite já usado em tb_client.Rntrc.
            migrationBuilder.AddColumn<string>(
                name: "Emitente_Rntrc",
                table: "tb_fiscalConfiguration",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_mdfe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Serie = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Numero = table.Column<long>(type: "bigint", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    Protocolo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UfCarregamento = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    UfDescarregamento = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    TipoEmitente = table.Column<int>(type: "integer", nullable: false),
                    Modal = table.Column<int>(type: "integer", nullable: false),
                    TipoOperacao = table.Column<int>(type: "integer", nullable: false),
                    StatusMdfe = table.Column<int>(type: "integer", nullable: false),
                    Sent = table.Column<bool>(type: "boolean", nullable: false),
                    TryCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    XmlCompleto = table.Column<string>(type: "text", nullable: true),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    CodMunCarregamento = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    MunCarregamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    TipoCarga = table.Column<int>(type: "integer", nullable: false),
                    ProdutoPredominante = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    InfoAdFisco = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InfoComplementar = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    PesoBruto = table.Column<decimal>(type: "numeric(15,3)", precision: 15, scale: 3, nullable: false),
                    QuantidadeNFe = table.Column<int>(type: "integer", nullable: false),
                    CodigoCIOT = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    ContratanteId = table.Column<int>(type: "integer", nullable: true),
                    IndicadorPagamento = table.Column<int>(type: "integer", nullable: false),
                    PagamentoBanco = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    PagamentoAgencia = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PagamentoCnpjIpef = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    PagamentoChavePix = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    IdVeiculoTracao = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_mdfe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_mdfe_tb_client_ContratanteId",
                        column: x => x.ContratanteId,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_mdfe_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_mdfe_tb_vehicle_IdVeiculoTracao",
                        column: x => x.IdVeiculoTracao,
                        principalTable: "tb_vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_mdfeCondutor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMdfe = table.Column<int>(type: "integer", nullable: false),
                    IdClient = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_mdfeCondutor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_mdfeCondutor_tb_client_IdClient",
                        column: x => x.IdClient,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_mdfeCondutor_tb_mdfe_IdMdfe",
                        column: x => x.IdMdfe,
                        principalTable: "tb_mdfe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_mdfeDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMdfe = table.Column<int>(type: "integer", nullable: false),
                    TipoDocumento = table.Column<int>(type: "integer", nullable: false),
                    NFeEmissionId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseId = table.Column<int>(type: "integer", nullable: true),
                    ChaveNFe = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    Serie = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Numero = table.Column<long>(type: "bigint", nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PartnerName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    UfOrigem = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    UfDestino = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    CodMunDescarga = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    MunicipioDescarga = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ValorMercadoria = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    PesoBruto = table.Column<decimal>(type: "numeric(15,3)", precision: 15, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_mdfeDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_mdfeDocumento_tb_mdfe_IdMdfe",
                        column: x => x.IdMdfe,
                        principalTable: "tb_mdfe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_mdfeDocumento_tb_nfeEmission_NFeEmissionId",
                        column: x => x.NFeEmissionId,
                        principalTable: "tb_nfeEmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_mdfeDocumento_tb_purchase_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "tb_purchase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_mdfePercurso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMdfe = table.Column<int>(type: "integer", nullable: false),
                    UfPercurso = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_mdfePercurso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_mdfePercurso_tb_mdfe_IdMdfe",
                        column: x => x.IdMdfe,
                        principalTable: "tb_mdfe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_mdfeVeiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMdfe = table.Column<int>(type: "integer", nullable: false),
                    IdVehicle = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_mdfeVeiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_mdfeVeiculo_tb_mdfe_IdMdfe",
                        column: x => x.IdMdfe,
                        principalTable: "tb_mdfe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_mdfeVeiculo_tb_vehicle_IdVehicle",
                        column: x => x.IdVehicle,
                        principalTable: "tb_vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 129, "Fiscal", 129, null, "Visualizar MDF-e" },
                    { 130, "Fiscal", 130, null, "Emitir MDF-e" },
                    { 131, "Fiscal", 131, null, "Editar MDF-e" },
                    { 132, "Fiscal", 132, null, "Excluir MDF-e" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfe_ContratanteId",
                table: "tb_mdfe",
                column: "ContratanteId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfe_IdCompany",
                table: "tb_mdfe",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfe_IdCompany_ChaveAcesso",
                table: "tb_mdfe",
                columns: new[] { "IdCompany", "ChaveAcesso" },
                unique: true,
                filter: "\"ChaveAcesso\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfe_IdCompany_Serie_Numero",
                table: "tb_mdfe",
                columns: new[] { "IdCompany", "Serie", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfe_IdCompany_StatusMdfe",
                table: "tb_mdfe",
                columns: new[] { "IdCompany", "StatusMdfe" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfe_IdVeiculoTracao",
                table: "tb_mdfe",
                column: "IdVeiculoTracao");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeCondutor_IdClient",
                table: "tb_mdfeCondutor",
                column: "IdClient");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeCondutor_IdMdfe",
                table: "tb_mdfeCondutor",
                column: "IdMdfe");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeCondutor_IdMdfe_Cpf",
                table: "tb_mdfeCondutor",
                columns: new[] { "IdMdfe", "Cpf" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeDocumento_IdMdfe",
                table: "tb_mdfeDocumento",
                column: "IdMdfe");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeDocumento_IdMdfe_ChaveNFe",
                table: "tb_mdfeDocumento",
                columns: new[] { "IdMdfe", "ChaveNFe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeDocumento_NFeEmissionId",
                table: "tb_mdfeDocumento",
                column: "NFeEmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeDocumento_PurchaseId",
                table: "tb_mdfeDocumento",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfePercurso_IdMdfe",
                table: "tb_mdfePercurso",
                column: "IdMdfe");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeVeiculo_IdMdfe",
                table: "tb_mdfeVeiculo",
                column: "IdMdfe");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeVeiculo_IdMdfe_IdVehicle",
                table: "tb_mdfeVeiculo",
                columns: new[] { "IdMdfe", "IdVehicle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_mdfeVeiculo_IdVehicle",
                table: "tb_mdfeVeiculo",
                column: "IdVehicle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_mdfeCondutor");

            migrationBuilder.DropTable(
                name: "tb_mdfeDocumento");

            migrationBuilder.DropTable(
                name: "tb_mdfePercurso");

            migrationBuilder.DropTable(
                name: "tb_mdfeVeiculo");

            migrationBuilder.DropTable(
                name: "tb_mdfe");

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DropColumn(
                name: "Emitente_Rntrc",
                table: "tb_fiscalConfiguration");

            migrationBuilder.DropColumn(
                name: "Mdfe_NumeroInicial",
                table: "tb_fiscalConfiguration");

            migrationBuilder.DropColumn(
                name: "Mdfe_Serie",
                table: "tb_fiscalConfiguration");

            // Os 3 AlterColumn de tb_driver_license que o scaffold gerou aqui
            // foram removidos: o Down não deve desfazer uma conversão de coluna
            // que o Up não fez. Ver o resumo da classe.
        }
    }
}
