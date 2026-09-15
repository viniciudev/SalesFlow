using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ServiceOrderNfse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_serviceOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Competence = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssqnValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssqnRetidoValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RetentionValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConcludedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_serviceOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_serviceOrder_tb_client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_serviceOrder_tb_company_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_serviceInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceOrderId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    IdDPS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChaveAcesso = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoAmbiente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DhEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CodMunIBGE = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NumeroDPS = table.Column<int>(type: "integer", nullable: false),
                    DataCompetencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssqnValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssqnRetidoValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RetentionValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CanceledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    XmlNfse = table.Column<string>(type: "text", nullable: true),
                    Sent = table.Column<bool>(type: "boolean", nullable: false),
                    TryCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RequestPayloadJson = table.Column<string>(type: "text", nullable: true),
                    ResponseJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_serviceInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_serviceInvoice_tb_client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_serviceInvoice_tb_company_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_serviceInvoice_tb_serviceOrder_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "tb_serviceOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_serviceOrderItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceOrderId = table.Column<int>(type: "integer", nullable: false),
                    ServiceProvidedId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssqnRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IssqnRetido = table.Column<bool>(type: "boolean", nullable: false),
                    PisRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CofinsRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IrRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CsllRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    InssRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_serviceOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_serviceOrderItem_tb_ServiceProvided_ServiceProvidedId",
                        column: x => x.ServiceProvidedId,
                        principalTable: "tb_ServiceProvided",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_serviceOrderItem_tb_serviceOrder_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "tb_serviceOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_serviceInvoiceItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ServiceProvidedId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IssqnRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IssqnRetido = table.Column<bool>(type: "boolean", nullable: false),
                    PisRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CofinsRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IrRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CsllRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    InssRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_serviceInvoiceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_serviceInvoiceItem_tb_ServiceProvided_ServiceProvidedId",
                        column: x => x.ServiceProvidedId,
                        principalTable: "tb_ServiceProvided",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_serviceInvoiceItem_tb_serviceInvoice_ServiceInvoiceId",
                        column: x => x.ServiceInvoiceId,
                        principalTable: "tb_serviceInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoice_ChaveAcesso",
                table: "tb_serviceInvoice",
                column: "ChaveAcesso");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoice_ClientId",
                table: "tb_serviceInvoice",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoice_IdDPS",
                table: "tb_serviceInvoice",
                column: "IdDPS");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoice_ServiceOrderId",
                table: "tb_serviceInvoice",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoice_TenantId_Status",
                table: "tb_serviceInvoice",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoiceItem_ServiceInvoiceId",
                table: "tb_serviceInvoiceItem",
                column: "ServiceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceInvoiceItem_ServiceProvidedId",
                table: "tb_serviceInvoiceItem",
                column: "ServiceProvidedId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceOrder_ClientId",
                table: "tb_serviceOrder",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceOrder_OrderDate",
                table: "tb_serviceOrder",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceOrder_TenantId_Status",
                table: "tb_serviceOrder",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceOrderItem_ServiceOrderId",
                table: "tb_serviceOrderItem",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceOrderItem_ServiceProvidedId",
                table: "tb_serviceOrderItem",
                column: "ServiceProvidedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_serviceInvoiceItem");

            migrationBuilder.DropTable(
                name: "tb_serviceOrderItem");

            migrationBuilder.DropTable(
                name: "tb_serviceInvoice");

            migrationBuilder.DropTable(
                name: "tb_serviceOrder");
        }
    }
}
