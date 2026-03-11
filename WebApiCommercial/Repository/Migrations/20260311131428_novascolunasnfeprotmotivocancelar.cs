using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class novascolunasnfeprotmotivocancelar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelamento",
                table: "tb_nfeEmission",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocolo",
                table: "tb_nfeEmission",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoCancelamento",
                table: "tb_nfeEmission");

            migrationBuilder.DropColumn(
                name: "Protocolo",
                table: "tb_nfeEmission");
        }
    }
}
