using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_budget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    IdClient = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_budget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_budget_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_client",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CellPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Complement = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NameState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_client", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_client_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_costCenter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCompany = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_costCenter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_costCenter_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_descriptionFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameProduct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descriptionProduct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valueProduct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    groupItems = table.Column<int>(type: "int", nullable: false),
                    idCompany = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_descriptionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_descriptionFiles_tb_company_idCompany",
                        column: x => x.idCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_planCompany",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    DateRegister = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPayment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_planCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_planCompany_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_product_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_prospects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_prospects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_prospects_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_salesman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCompany = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_salesman", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_salesman_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_ServiceProvided",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_ServiceProvided", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_ServiceProvided_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_user",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCompany = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_user_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_budgetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdBudget = table.Column<int>(type: "int", nullable: false),
                    TypeItem = table.Column<int>(type: "int", nullable: false),
                    IdItem = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_budgetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_budgetItems_tb_budget_IdBudget",
                        column: x => x.IdBudget,
                        principalTable: "tb_budget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_serviceProvision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdBudget = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdClient = table.Column<int>(type: "int", nullable: false),
                    IdCompany = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_serviceProvision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_serviceProvision_tb_budget_IdBudget",
                        column: x => x.IdBudget,
                        principalTable: "tb_budget",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_serviceProvision_tb_client_IdClient",
                        column: x => x.IdClient,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_serviceProvision_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_file",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Files = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileThumb = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IdDescriptionFiles = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_file", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_file_tb_descriptionFiles_IdDescriptionFiles",
                        column: x => x.IdDescriptionFiles,
                        principalTable: "tb_descriptionFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_phasesProspects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProspects = table.Column<int>(type: "int", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_phasesProspects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_phasesProspects_tb_prospects_IdProspects",
                        column: x => x.IdProspects,
                        principalTable: "tb_prospects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_closures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LongInit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LatInit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LongFinal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LatFinal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kilometerTraveled = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Odometer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateFinal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdSalesman = table.Column<int>(type: "int", nullable: false),
                    OdometerFinal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_closures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_closures_tb_salesman_IdSalesman",
                        column: x => x.IdSalesman,
                        principalTable: "tb_salesman",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_sale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdClient = table.Column<int>(type: "int", nullable: false),
                    IdSeller = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_sale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_sale_tb_client_IdClient",
                        column: x => x.IdClient,
                        principalTable: "tb_client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_sale_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_sale_tb_salesman_IdSeller",
                        column: x => x.IdSeller,
                        principalTable: "tb_salesman",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tb_commission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdService = table.Column<int>(type: "int", nullable: true),
                    IdProduct = table.Column<int>(type: "int", nullable: true),
                    IdSalesman = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CommissionDay = table.Column<int>(type: "int", nullable: false),
                    TypeDay = table.Column<int>(type: "int", nullable: false),
                    IdCostCenter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_commission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_commission_tb_costCenter_IdCostCenter",
                        column: x => x.IdCostCenter,
                        principalTable: "tb_costCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_commission_tb_product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "tb_product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_commission_tb_salesman_IdSalesman",
                        column: x => x.IdSalesman,
                        principalTable: "tb_salesman",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_commission_tb_ServiceProvided_IdService",
                        column: x => x.IdService,
                        principalTable: "tb_ServiceProvided",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tb_servicesProvisionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdServiceProvision = table.Column<int>(type: "int", nullable: false),
                    TypeItem = table.Column<int>(type: "int", nullable: false),
                    IdItem = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_servicesProvisionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_servicesProvisionItems_tb_serviceProvision_IdServiceProvision",
                        column: x => x.IdServiceProvision,
                        principalTable: "tb_serviceProvision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_closuresDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdClosures = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_closuresDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_closuresDetail_tb_closures_IdClosures",
                        column: x => x.IdClosures,
                        principalTable: "tb_closures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_saleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    InclusionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdSale = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: true),
                    IdService = table.Column<int>(type: "int", nullable: true),
                    TypeItem = table.Column<int>(type: "int", nullable: false),
                    RecurringAmount = table.Column<int>(type: "int", nullable: false),
                    EnableRecurrence = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_saleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_saleItems_tb_product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "tb_product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_saleItems_tb_sale_IdSale",
                        column: x => x.IdSale,
                        principalTable: "tb_sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_saleItems_tb_ServiceProvided_IdService",
                        column: x => x.IdService,
                        principalTable: "tb_ServiceProvided",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tb_financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialType = table.Column<int>(type: "int", nullable: false),
                    IdCostCenter = table.Column<int>(type: "int", nullable: false),
                    FinancialStatus = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    IdSalesman = table.Column<int>(type: "int", nullable: true),
                    IdProduct = table.Column<int>(type: "int", nullable: true),
                    IdService = table.Column<int>(type: "int", nullable: true),
                    IdSale = table.Column<int>(type: "int", nullable: true),
                    IdSaleItems = table.Column<int>(type: "int", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    commission = table.Column<bool>(type: "bit", nullable: false),
                    Origin = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_financial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_company_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "tb_company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_costCenter_IdCostCenter",
                        column: x => x.IdCostCenter,
                        principalTable: "tb_costCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_product_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "tb_product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_sale_IdSale",
                        column: x => x.IdSale,
                        principalTable: "tb_sale",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_saleItems_IdSaleItems",
                        column: x => x.IdSaleItems,
                        principalTable: "tb_saleItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_salesman_IdSalesman",
                        column: x => x.IdSalesman,
                        principalTable: "tb_salesman",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_financial_tb_ServiceProvided_IdService",
                        column: x => x.IdService,
                        principalTable: "tb_ServiceProvided",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tb_sharedCommission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSaleItems = table.Column<int>(type: "int", nullable: false),
                    IdSalesman = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionDay = table.Column<int>(type: "int", nullable: false),
                    TypeDay = table.Column<int>(type: "int", nullable: false),
                    IdCostCenter = table.Column<int>(type: "int", nullable: false),
                    EnableSharedCommission = table.Column<bool>(type: "bit", nullable: false),
                    RecurringAmount = table.Column<int>(type: "int", nullable: false),
                    NameSeller = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_sharedCommission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_sharedCommission_tb_costCenter_IdCostCenter",
                        column: x => x.IdCostCenter,
                        principalTable: "tb_costCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_sharedCommission_tb_saleItems_IdSaleItems",
                        column: x => x.IdSaleItems,
                        principalTable: "tb_saleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_sharedCommission_tb_salesman_IdSalesman",
                        column: x => x.IdSalesman,
                        principalTable: "tb_salesman",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "tb_company",
                columns: new[] { "Id", "CorporateName", "Guid" },
                values: new object[] { 1, "Empresa Padrão", new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.InsertData(
                table: "tb_user",
                columns: new[] { "Id", "BirthDate", "CellPhone", "Email", "IdCompany", "Name", "Password", "Role" },
                values: new object[] { 1, new DateTime(1983, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@padrao.com.br", 1, "Admin", "", null });

            migrationBuilder.CreateIndex(
                name: "IX_tb_budget_IdCompany",
                table: "tb_budget",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_budgetItems_IdBudget",
                table: "tb_budgetItems",
                column: "IdBudget");

            migrationBuilder.CreateIndex(
                name: "IX_tb_client_IdCompany",
                table: "tb_client",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_closures_IdSalesman",
                table: "tb_closures",
                column: "IdSalesman");

            migrationBuilder.CreateIndex(
                name: "IX_tb_closuresDetail_IdClosures",
                table: "tb_closuresDetail",
                column: "IdClosures");

            migrationBuilder.CreateIndex(
                name: "IX_tb_commission_IdCostCenter",
                table: "tb_commission",
                column: "IdCostCenter");

            migrationBuilder.CreateIndex(
                name: "IX_tb_commission_IdProduct",
                table: "tb_commission",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_tb_commission_IdSalesman",
                table: "tb_commission",
                column: "IdSalesman");

            migrationBuilder.CreateIndex(
                name: "IX_tb_commission_IdService",
                table: "tb_commission",
                column: "IdService");

            migrationBuilder.CreateIndex(
                name: "IX_tb_costCenter_IdCompany",
                table: "tb_costCenter",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_descriptionFiles_idCompany",
                table: "tb_descriptionFiles",
                column: "idCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_file_IdDescriptionFiles",
                table: "tb_file",
                column: "IdDescriptionFiles");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdCompany",
                table: "tb_financial",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdCostCenter",
                table: "tb_financial",
                column: "IdCostCenter");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdProduct",
                table: "tb_financial",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdSale",
                table: "tb_financial",
                column: "IdSale");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdSaleItems",
                table: "tb_financial",
                column: "IdSaleItems");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdSalesman",
                table: "tb_financial",
                column: "IdSalesman");

            migrationBuilder.CreateIndex(
                name: "IX_tb_financial_IdService",
                table: "tb_financial",
                column: "IdService");

            migrationBuilder.CreateIndex(
                name: "IX_tb_phasesProspects_IdProspects",
                table: "tb_phasesProspects",
                column: "IdProspects");

            migrationBuilder.CreateIndex(
                name: "IX_tb_planCompany_IdCompany",
                table: "tb_planCompany",
                column: "IdCompany",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_product_IdCompany",
                table: "tb_product",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_prospects_IdCompany",
                table: "tb_prospects",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_sale_IdClient",
                table: "tb_sale",
                column: "IdClient");

            migrationBuilder.CreateIndex(
                name: "IX_tb_sale_IdCompany",
                table: "tb_sale",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_sale_IdSeller",
                table: "tb_sale",
                column: "IdSeller");

            migrationBuilder.CreateIndex(
                name: "IX_tb_saleItems_IdProduct",
                table: "tb_saleItems",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_tb_saleItems_IdSale",
                table: "tb_saleItems",
                column: "IdSale");

            migrationBuilder.CreateIndex(
                name: "IX_tb_saleItems_IdService",
                table: "tb_saleItems",
                column: "IdService");

            migrationBuilder.CreateIndex(
                name: "IX_tb_salesman_IdCompany",
                table: "tb_salesman",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_ServiceProvided_IdCompany",
                table: "tb_ServiceProvided",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceProvision_IdBudget",
                table: "tb_serviceProvision",
                column: "IdBudget");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceProvision_IdClient",
                table: "tb_serviceProvision",
                column: "IdClient");

            migrationBuilder.CreateIndex(
                name: "IX_tb_serviceProvision_IdCompany",
                table: "tb_serviceProvision",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_tb_servicesProvisionItems_IdServiceProvision",
                table: "tb_servicesProvisionItems",
                column: "IdServiceProvision");

            migrationBuilder.CreateIndex(
                name: "IX_tb_sharedCommission_IdCostCenter",
                table: "tb_sharedCommission",
                column: "IdCostCenter");

            migrationBuilder.CreateIndex(
                name: "IX_tb_sharedCommission_IdSaleItems",
                table: "tb_sharedCommission",
                column: "IdSaleItems");

            migrationBuilder.CreateIndex(
                name: "IX_tb_sharedCommission_IdSalesman",
                table: "tb_sharedCommission",
                column: "IdSalesman");

            migrationBuilder.CreateIndex(
                name: "IX_tb_user_IdCompany",
                table: "tb_user",
                column: "IdCompany");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_budgetItems");

            migrationBuilder.DropTable(
                name: "tb_closuresDetail");

            migrationBuilder.DropTable(
                name: "tb_commission");

            migrationBuilder.DropTable(
                name: "tb_file");

            migrationBuilder.DropTable(
                name: "tb_financial");

            migrationBuilder.DropTable(
                name: "tb_phasesProspects");

            migrationBuilder.DropTable(
                name: "tb_planCompany");

            migrationBuilder.DropTable(
                name: "tb_servicesProvisionItems");

            migrationBuilder.DropTable(
                name: "tb_sharedCommission");

            migrationBuilder.DropTable(
                name: "tb_user");

            migrationBuilder.DropTable(
                name: "tb_closures");

            migrationBuilder.DropTable(
                name: "tb_descriptionFiles");

            migrationBuilder.DropTable(
                name: "tb_prospects");

            migrationBuilder.DropTable(
                name: "tb_serviceProvision");

            migrationBuilder.DropTable(
                name: "tb_costCenter");

            migrationBuilder.DropTable(
                name: "tb_saleItems");

            migrationBuilder.DropTable(
                name: "tb_budget");

            migrationBuilder.DropTable(
                name: "tb_product");

            migrationBuilder.DropTable(
                name: "tb_sale");

            migrationBuilder.DropTable(
                name: "tb_ServiceProvided");

            migrationBuilder.DropTable(
                name: "tb_client");

            migrationBuilder.DropTable(
                name: "tb_salesman");

            migrationBuilder.DropTable(
                name: "tb_company");
        }
    }
}
