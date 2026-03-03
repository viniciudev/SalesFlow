using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ajustetboxcolunaempresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_box_tb_company_CompanyId",
                table: "tb_box");

            migrationBuilder.DropIndex(
                name: "IX_tb_box_CompanyId",
                table: "tb_box");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "tb_box");

            migrationBuilder.CreateIndex(
                name: "IX_tb_box_IdCompany",
                table: "tb_box",
                column: "IdCompany");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_box_tb_company_IdCompany",
                table: "tb_box",
                column: "IdCompany",
                principalTable: "tb_company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_box_tb_company_IdCompany",
                table: "tb_box");

            migrationBuilder.DropIndex(
                name: "IX_tb_box_IdCompany",
                table: "tb_box");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "tb_box",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_box_CompanyId",
                table: "tb_box",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_box_tb_company_CompanyId",
                table: "tb_box",
                column: "CompanyId",
                principalTable: "tb_company",
                principalColumn: "Id");
        }
    }
}
