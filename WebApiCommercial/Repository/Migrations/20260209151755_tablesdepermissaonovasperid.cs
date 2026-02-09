using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tablesdepermissaonovasperid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "tb_permission",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Cadastros", 10, "Visualizar Produtos" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Cadastros", 11, "Criar Produto" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Cadastros", 12, "Editar Produto" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Cadastros", 13, "Excluir Produto" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Cadastros", 20, "Visualizar Clientes" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Category", "Code", "Description", "Name" },
                values: new object[] { "Cadastros", 21, null, "Criar Cliente" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Code", "Name" },
                values: new object[] { 22, "Editar Cliente" });

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 30, "Vendas", 30, null, "Visualizar Vendas" },
                    { 31, "Vendas", 31, null, "Criar Venda" },
                    { 32, "Vendas", 32, null, "Cancelar Venda" },
                    { 40, "Financeiro", 40, null, "Visualizar Financeiro" },
                    { 41, "Financeiro", 41, null, "Editar Financeiro" },
                    { 50, "Estoque", 50, null, "Visualizar Estoque" },
                    { 51, "Estoque", 51, null, "Ajustar Estoque" },
                    { 60, "Renegociação", 60, null, "Visualizar Renegociações" },
                    { 61, "Renegociação", 61, null, "Criar Renegociação" },
                    { 70, "Usuários", 70, null, "Visualizar Usuários" },
                    { 71, "Usuários", 71, null, "Gerenciar Usuários" },
                    { 72, "Usuários", 72, "Pode atribuir permissões a outros usuários", "Gerenciar Permissões" },
                    { 80, "Cadastros", 80, null, "Visualizar Formas de Pagamento" },
                    { 81, "Cadastros", 81, null, "Gerenciar Formas de Pagamento" },
                    { 82, "Vendas", 82, null, "Alterar Venda" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "tb_permission",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Vendas", 30, "Visualizar Vendas" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Vendas", 31, "Criar Venda" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Vendas", 32, "Cancelar Venda" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Financeiro", 40, "Visualizar Financeiro" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Category", "Code", "Name" },
                values: new object[] { "Usuários", 71, "Gerenciar Usuários" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Category", "Code", "Description", "Name" },
                values: new object[] { "Usuários", 72, "Pode atribuir permissões a outros usuários", "Gerenciar Permissões" });

            migrationBuilder.UpdateData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Code", "Name" },
                values: new object[] { 80, "Visualizar Formas de Pagamento" });

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 3, "Cadastros", 10, null, "Visualizar Produtos" },
                    { 4, "Cadastros", 11, null, "Criar Produto" },
                    { 5, "Cadastros", 12, null, "Editar Produto" },
                    { 6, "Cadastros", 13, null, "Excluir Produto" },
                    { 7, "Cadastros", 20, null, "Visualizar Clientes" },
                    { 8, "Cadastros", 21, null, "Criar Cliente" },
                    { 9, "Cadastros", 22, null, "Editar Cliente" },
                    { 14, "Financeiro", 41, null, "Editar Financeiro" },
                    { 15, "Estoque", 50, null, "Visualizar Estoque" },
                    { 16, "Estoque", 51, null, "Ajustar Estoque" },
                    { 17, "Renegociação", 60, null, "Visualizar Renegociações" },
                    { 18, "Renegociação", 61, null, "Criar Renegociação" },
                    { 19, "Usuários", 70, null, "Visualizar Usuários" },
                    { 23, "Cadastros", 81, null, "Gerenciar Formas de Pagamento" },
                    { 24, "Cadastros", 81, null, "Gerenciar Formas de Pagamento" }
                });
        }
    }
}
