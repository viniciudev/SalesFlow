using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tablecontabancaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_bankAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    BankCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgencyNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AgencyDigit = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccountDigit = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    HolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HolderDocument = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HolderType = table.Column<int>(type: "integer", nullable: false),
                    PixKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_bankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_bankAccount_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_bankAccount_IdCompany",
                table: "tb_bankAccount",
                column: "IdCompany");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_bankAccount");
        }
    }
}
