using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
	public partial class addinstallmentduedatessalepayment : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
                ALTER TABLE ""tb_salePayment""
                ADD COLUMN IF NOT EXISTS ""InstallmentDueDatesJson"" text;
            ");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
                ALTER TABLE ""tb_salePayment""
                DROP COLUMN IF EXISTS ""InstallmentDueDatesJson"";
            ");
		}
	}
}