using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class addcolunsanddetailsservice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Capacity",
                table: "tb_ServiceProvided",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Deadline",
                table: "tb_ServiceProvided",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "tb_ServiceProvided",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_detailsService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdServiceProvided = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_detailsService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_detailsService_tb_ServiceProvided_IdServiceProvided",
                        column: x => x.IdServiceProvided,
                        principalTable: "tb_ServiceProvided",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_detailsService_IdServiceProvided",
                table: "tb_detailsService",
                column: "IdServiceProvided");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_detailsService");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "tb_ServiceProvided");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "tb_ServiceProvided");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "tb_ServiceProvided");
        }
    }
}
