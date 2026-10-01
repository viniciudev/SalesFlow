using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <summary>
    /// Cadastro de veículos para MDF-e (OS-002).
    ///
    /// <b>Esta migration foi revisada à mão.</b> O scaffold do EF gerou, além do
    /// que está aqui, 34 <c>AlterColumn</c> de data em 20 tabelas de outros
    /// módulos (tb_user, tb_stock, tb_budget, tb_client…). Foram removidos.
    ///
    /// Por que eles aparecem — importante para quem for gerar a próxima: o
    /// <c>ContextBaseModelSnapshot</c> estava obsoleto, registrando
    /// <c>timestamp without time zone</c> em 34 colunas que no banco são
    /// <c>timestamp with time zone</c> (44 das 47 colunas de data do banco são
    /// timestamptz; as 3 exceções estão abaixo). O modelo de design time mapeia
    /// <c>DateTime</c> para timestamptz, então o diff acusava "conversão" de
    /// colunas que já estavam no tipo certo.
    ///
    /// O snapshot foi corrigido para espelhar o banco, e o diff caiu de 34
    /// colunas para as 3 de baixo.
    ///
    /// <b>Hipótese descartada por medição</b> (registrada para não ser
    /// retomada): a suspeita de que <c>AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)</c>
    /// em <c>Startup.cs</c> vazasse para o design time. Não vaza —
    /// <c>Repository/DesignTimeContextFactory.cs</c> implementa
    /// <c>IDesignTimeDbContextFactory&lt;ContextBase&gt;</c>, então o EF constrói o
    /// contexto direto e NUNCA executa o <c>Startup</c>. Medido: comentar aquela
    /// linha e rodar <c>migrations add</c> produz exatamente o mesmo scaffold, as
    /// mesmas 3 colunas. (O teste foi feito e desfeito; a linha está de volta —
    /// não a deixe comentada, ela é comportamento de RUNTIME.)
    ///
    /// Resta UMA divergência real, que esta migration deliberadamente não resolve
    /// porque é de outro módulo: <c>tb_driver_license.DataEmissaoCnh</c>,
    /// <c>DataValidadeCnh</c> e <c>PrimeiraHabilitacao</c> são
    /// <c>timestamp without time zone</c> no banco e timestamptz no modelo. Ou
    /// seja, o banco é MISTO. Para zerar o drift, declarar
    /// <c>HasColumnType("timestamp without time zone")</c> nessas 3 propriedades em
    /// <c>ConfiguraDriverLicense</c> (nenhum efeito em runtime; só alinha o
    /// modelo ao banco). O próximo <c>migrations add</c> vai propor esses 3
    /// ALTERs — decidir se aplica essa declaração ou se converte as colunas.
    ///
    /// Conteúdo real: 2 tabelas novas, 1 coluna nova em <c>tb_client</c>
    /// (RNTRC), 5 índices e o seed das 4 permissões.
    /// </summary>
    public partial class AddVehicleMdfe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RV10 — o veículo de terceiro precisa do RNTRC do proprietário.
            // A coluna vai no parceiro (tb_client), reusando o cadastro que já
            // concentra CPF/CNPJ, IE e UF.
            //
            // 8 caracteres é o tipo TRNTRC dos XSDs do MDF-e
            // (`[0-9]{8}`), e não um limite folgado: deixar a coluna maior
            // permitiria gravar um RNTRC que só seria recusado na SEFAZ.
            migrationBuilder.AddColumn<string>(
                name: "Rntrc",
                table: "tb_client",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_vehicle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    InternalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LicensePlate = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Renavam = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    LicensingState = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    WheelType = table.Column<int>(type: "integer", nullable: false),
                    BodyType = table.Column<int>(type: "integer", nullable: false),
                    Tare = table.Column<int>(type: "integer", nullable: false),
                    CapacityKg = table.Column<int>(type: "integer", nullable: true),
                    CapacityM3 = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    OwnershipType = table.Column<int>(type: "integer", nullable: false),
                    ThirdPartyCategory = table.Column<int>(type: "integer", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_vehicle_tb_client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_vehicle_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // RV14 — histórico de uso. Nasce vazia: a emissão de MDF-e ainda não
            // existe. É a primeira tabela de histórico do projeto.
            migrationBuilder.CreateTable(
                name: "tb_vehicleUsageHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdVehicle = table.Column<int>(type: "integer", nullable: false),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: true),
                    Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_vehicleUsageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_vehicleUsageHistory_tb_vehicle_IdVehicle",
                        column: x => x.IdVehicle,
                        principalTable: "tb_vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Permissões do módulo. O middleware de permissão deriva o código do
            // verbo HTTP, então os 4 são necessários (VIEW/CREATE/EDIT/DELETE).
            // Sem eles o usuário não consegue liberar acesso à tela.
            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 125, "Cadastros", 125, null, "Visualizar Veículos" },
                    { 126, "Cadastros", 126, null, "Criar Veículo" },
                    { 127, "Cadastros", 127, null, "Editar Veículo" },
                    { 128, "Cadastros", 128, null, "Desativar Veículo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicle_ClientId",
                table: "tb_vehicle",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicle_IdCompany",
                table: "tb_vehicle",
                column: "IdCompany");

            // RV02 — placa única apenas entre veículos ATIVOS, por empresa.
            // O filtro parcial é o que faz a exclusão lógica (RV11) liberar a
            // placa para recadastro. Sintaxe de filtro do POSTGRES, não a do
            // SQL Server ("[IsActive] = 1" não funcionaria aqui).
            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicle_IdCompany_LicensePlate",
                table: "tb_vehicle",
                columns: new[] { "IdCompany", "LicensePlate" },
                unique: true,
                filter: "\"IsActive\"");

            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicleUsageHistory_IdCompany_Source",
                table: "tb_vehicleUsageHistory",
                columns: new[] { "IdCompany", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_vehicleUsageHistory_IdVehicle",
                table: "tb_vehicleUsageHistory",
                column: "IdVehicle");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_vehicleUsageHistory");

            migrationBuilder.DropTable(
                name: "tb_vehicle");

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DropColumn(
                name: "Rntrc",
                table: "tb_client");
        }
    }
}
