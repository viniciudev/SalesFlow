using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class indexfalseclientedocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DROP INDEX IF EXISTS ""IX_tb_client_IdCompany_Document"";
    ");

            migrationBuilder.CreateIndex(
                name: "IX_tb_client_IdCompany_Document",
                table: "tb_client",
                columns: new[] { "IdCompany", "Document" },
                filter: "\"Document\" <> ''");
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DROP INDEX IF EXISTS ""IX_tb_client_IdCompany_Document"";
    ");

            migrationBuilder.CreateIndex(
                name: "IX_tb_client_IdCompany_Document",
                table: "tb_client",
                columns: new[] { "IdCompany", "Document" },
                unique: true,
                filter: "\"Document\" <> ''");
        }
    }
}
