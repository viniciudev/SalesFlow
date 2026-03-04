using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class correcaocolunaidcompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_nfeEmission_tb_company_ComapanyId",
                table: "tb_nfeEmission");

            migrationBuilder.RenameColumn(
                name: "ComapanyId",
                table: "tb_nfeEmission",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_nfeEmission_ComapanyId",
                table: "tb_nfeEmission",
                newName: "IX_tb_nfeEmission_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_nfeEmission_tb_company_CompanyId",
                table: "tb_nfeEmission",
                column: "CompanyId",
                principalTable: "tb_company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_nfeEmission_tb_company_CompanyId",
                table: "tb_nfeEmission");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "tb_nfeEmission",
                newName: "ComapanyId");

            migrationBuilder.RenameIndex(
                name: "IX_tb_nfeEmission_CompanyId",
                table: "tb_nfeEmission",
                newName: "IX_tb_nfeEmission_ComapanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_nfeEmission_tb_company_ComapanyId",
                table: "tb_nfeEmission",
                column: "ComapanyId",
                principalTable: "tb_company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
