using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ajustefornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_purchase_tb_provider_FornecedorId",
                table: "tb_purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_provider",
                table: "tb_provider");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "tb_provider");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "tb_provider",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "tb_provider",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_provider",
                table: "tb_provider",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_purchase_tb_provider_FornecedorId",
                table: "tb_purchase",
                column: "FornecedorId",
                principalTable: "tb_provider",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_purchase_tb_provider_FornecedorId",
                table: "tb_purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_provider",
                table: "tb_provider");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tb_provider",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "tb_provider",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "tb_provider",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_provider",
                table: "tb_provider",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_purchase_tb_provider_FornecedorId",
                table: "tb_purchase",
                column: "FornecedorId",
                principalTable: "tb_provider",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
