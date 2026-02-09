using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class tablesdepermissaonovacolunacategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "tb_permission",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "tb_permission",
                columns: new[] { "Id", "Category", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { -23, "Cadastros", 81, null, "Gerenciar Formas de Pagamento" },
                    { -22, "Cadastros", 80, null, "Visualizar Formas de Pagamento" },
                    { -21, "Usuários", 72, "Pode atribuir permissões a outros usuários", "Gerenciar Permissões" },
                    { -20, "Usuários", 71, null, "Gerenciar Usuários" },
                    { -19, "Usuários", 70, null, "Visualizar Usuários" },
                    { -18, "Renegociação", 61, null, "Criar Renegociação" },
                    { -17, "Renegociação", 60, null, "Visualizar Renegociações" },
                    { -16, "Estoque", 51, null, "Ajustar Estoque" },
                    { -15, "Estoque", 50, null, "Visualizar Estoque" },
                    { -14, "Financeiro", 41, null, "Editar Financeiro" },
                    { -13, "Financeiro", 40, null, "Visualizar Financeiro" },
                    { -12, "Vendas", 32, null, "Cancelar Venda" },
                    { -11, "Vendas", 31, null, "Criar Venda" },
                    { -10, "Vendas", 30, null, "Visualizar Vendas" },
                    { -9, "Cadastros", 22, null, "Editar Cliente" },
                    { -8, "Cadastros", 21, null, "Criar Cliente" },
                    { -7, "Cadastros", 20, null, "Visualizar Clientes" },
                    { -6, "Cadastros", 13, null, "Excluir Produto" },
                    { -5, "Cadastros", 12, null, "Editar Produto" },
                    { -4, "Cadastros", 11, null, "Criar Produto" },
                    { -3, "Cadastros", 10, null, "Visualizar Produtos" },
                    { -2, "Cadastros", 2, null, "Editar Empresa" },
                    { -1, "Cadastros", 1, null, "Visualizar Empresa" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_permission_Code",
                table: "tb_permission",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tb_permission_Code",
                table: "tb_permission");

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -23);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -22);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -21);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -20);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -19);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -18);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -17);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -14);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -13);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -12);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -11);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -10);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -9);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -8);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -7);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -6);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -5);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "tb_permission",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "tb_permission");
        }
    }
}
