using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class novacolunapagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowInstallments",
                table: "tb_paymentMethod",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImmediateSettlement",
                table: "tb_paymentMethod",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Installments",
                table: "tb_financial",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowInstallments",
                table: "tb_paymentMethod");

            migrationBuilder.DropColumn(
                name: "IsImmediateSettlement",
                table: "tb_paymentMethod");

            migrationBuilder.DropColumn(
                name: "Installments",
                table: "tb_financial");
        }
    }
}
