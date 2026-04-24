using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class clientNullnavenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_sale_tb_client_IdClient",
                table: "tb_sale");

            migrationBuilder.AlterColumn<int>(
                name: "IdClient",
                table: "tb_sale",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_sale_tb_client_IdClient",
                table: "tb_sale",
                column: "IdClient",
                principalTable: "tb_client",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_sale_tb_client_IdClient",
                table: "tb_sale");

            migrationBuilder.AlterColumn<int>(
                name: "IdClient",
                table: "tb_sale",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_sale_tb_client_IdClient",
                table: "tb_sale",
                column: "IdClient",
                principalTable: "tb_client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
