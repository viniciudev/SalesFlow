using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class altercollunsfinancial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_costCenter_IdCostCenter",
                table: "tb_financial");

            migrationBuilder.AlterColumn<int>(
                name: "IdCostCenter",
                table: "tb_financial",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_costCenter_IdCostCenter",
                table: "tb_financial",
                column: "IdCostCenter",
                principalTable: "tb_costCenter",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_financial_tb_costCenter_IdCostCenter",
                table: "tb_financial");

            migrationBuilder.AlterColumn<int>(
                name: "IdCostCenter",
                table: "tb_financial",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_financial_tb_costCenter_IdCostCenter",
                table: "tb_financial",
                column: "IdCostCenter",
                principalTable: "tb_costCenter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
