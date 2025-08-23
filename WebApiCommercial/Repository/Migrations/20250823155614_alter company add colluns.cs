using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class altercompanyaddcolluns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cellphone",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialPhone",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "tb_company",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "Cellphone",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "City",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "CommercialPhone",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "State",
                table: "tb_company");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "tb_company");
        }
    }
}
