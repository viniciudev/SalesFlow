using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Novoscamposfinanceiroparacontrolejuros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FineValue",
                table: "tb_financial",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestValue",
                table: "tb_financial",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SettledValue",
                table: "tb_financial",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SettlementDate",
                table: "tb_financial",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FineValue",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "InterestValue",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "SettledValue",
                table: "tb_financial");

            migrationBuilder.DropColumn(
                name: "SettlementDate",
                table: "tb_financial");
        }
    }
}
