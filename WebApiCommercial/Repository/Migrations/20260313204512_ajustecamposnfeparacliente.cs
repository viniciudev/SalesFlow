using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ajustecamposnfeparacliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoMunicipio",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "tb_client");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "tb_client",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "tb_client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CellPhone",
                table: "tb_client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "tb_client",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                table: "tb_client",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "tb_client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "tb_client",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodMunicipioIbge",
                table: "tb_client",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodPais",
                table: "tb_client",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "tb_client",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumidorFinal",
                table: "tb_client",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CpfCnpj",
                table: "tb_client",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ie",
                table: "tb_client",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndicadorIE",
                table: "tb_client",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InscricaoMunicipal",
                table: "tb_client",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "tb_client",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "tb_client",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomeRazao",
                table: "tb_client",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "tb_client",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "tb_client",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPessoa",
                table: "tb_client",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "tb_client",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cep",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "CodMunicipioIbge",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "CodPais",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "ConsumidorFinal",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "CpfCnpj",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Ie",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "IndicadorIE",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "InscricaoMunicipal",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "NomeRazao",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Pais",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "TipoPessoa",
                table: "tb_client");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "tb_client");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "tb_client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "tb_client",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CellPhone",
                table: "tb_client",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "tb_client",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                table: "tb_client",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "tb_client",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CodigoMunicipio",
                table: "tb_client",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "tb_client",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
