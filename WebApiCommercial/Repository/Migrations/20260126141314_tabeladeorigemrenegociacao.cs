using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tabeladeorigemrenegociacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_tb_financial_tb_client_ClientId",
            //    table: "tb_financial");

            //migrationBuilder.DropIndex(
            //    name: "IX_tb_financial_ClientId",
            //    table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "tb_financial");

            migrationBuilder.AlterColumn<int>(
                name: "IdClient",
                table: "tb_financial",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "tb_financialResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdRefOrigin = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_financialResources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdClient",
                table: "tb_financial",
                column: "IdClient");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tb_financial_tb_client_IdClient",
            //    table: "tb_financial",
            //    column: "IdClient",
            //    principalTable: "tb_client",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_tb_financial_tb_client_IdClient",
            //    table: "tb_financial");

            migrationBuilder.DropTable(
                name: "tb_financialResources");

            migrationBuilder.DropIndex(
                name: "IX_tb_financial_IdClient",
                table: "tb_financial");

            migrationBuilder.AlterColumn<int>(
                name: "IdClient",
                table: "tb_financial",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "tb_financial",
                type: "integer",
                nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_tb_financial_ClientId",
            //    table: "tb_financial",
            //    column: "ClientId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tb_financial_tb_client_ClientId",
            //    table: "tb_financial",
            //    column: "ClientId",
            //    principalTable: "tb_client",
            //    principalColumn: "Id");
        }
    }
}
