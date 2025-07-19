using Microsoft.EntityFrameworkCore;
using Model;
using Model.Closure;
using Model.Moves;
using Model.Registrations;
using System;
using System.Linq;

namespace Repository
{
  public class ContextBase : DbContext
  {
    public ContextBase()
    { }
    public ContextBase(DbContextOptions<ContextBase> opcoes) : base(opcoes)
    {

    }
    public virtual DbSet<User> User { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {
        optionsBuilder.UseSqlServer(@"Server=.\sqlexpress;Database=serviceboxdb;Trusted_Connection=True;");
      }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      ConfiguraCompany(modelBuilder);
      ConfiguraEmpresa(modelBuilder);
      ConfiguraClient(modelBuilder);
      ConfiguraFile(modelBuilder);
      ConfiguraDescriptionFiles(modelBuilder);
      ConfiguraProduct(modelBuilder);
      ConfiguraService(modelBuilder);
      ConfiguraBudget(modelBuilder);
      ConfiguraBudgetItems(modelBuilder);
      ConfiguraServiceProvision(modelBuilder);
      ConfiguraServiceProvisionItems(modelBuilder);
      ConfiguraSalesman(modelBuilder);
      ConfiguraSale(modelBuilder);
      ConfiguraSaleItems(modelBuilder);
      ConfiguraCommission(modelBuilder);
      ConfiguraCostCenter(modelBuilder);
      ConfiguraFinancial(modelBuilder);
      ConfiguraPlanCompany(modelBuilder);
      ConfiguraProspects(modelBuilder);
      ConfiguraPhasesProspects(modelBuilder);
      ConfiguraSharedCommission(modelBuilder);
      ConfiguraClosuresDetail(modelBuilder);
      ConfiguraClosures(modelBuilder);
      var cascadeFKs = modelBuilder.Model.GetEntityTypes()
     .SelectMany(t => t.GetForeignKeys())
     .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);
      foreach (var fk in cascadeFKs)
        fk.DeleteBehavior = DeleteBehavior.Restrict;

      base.OnModelCreating(modelBuilder);
    }

    private void ConfiguraEmpresa(ModelBuilder builder)
    {
      builder.Entity<User>(user =>
      {
        user.ToTable("tb_user");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
        user.Property(c => c.Name).HasMaxLength(100);
        user.Property(c => c.Password).HasMaxLength(150);
        user.Property(c => c.Email).HasMaxLength(100);
      });
      builder.Entity<User>()
       .HasOne(dc => dc.Company)
       .WithMany(c => c.Users)
       .HasForeignKey(dc => dc.IdCompany);
      builder.Entity<User>().HasData(new User { Id = 1, Email = "admin@padrao.com.br", Name = "Admin", Password = "", BirthDate = new DateTime(1983, 1, 1), IdCompany = 1 });
    }


    private void ConfiguraClient(ModelBuilder builder)
    {
      builder.Entity<Client>(client =>
      {
        client.ToTable("tb_client");
        client.HasKey(c => c.Id);
        client.Property(c => c.Id).ValueGeneratedOnAdd();
        client.Property(c => c.Name).HasMaxLength(150);
        client.Property(c => c.Email).HasMaxLength(100);
        client.Property(c => c.CellPhone).HasMaxLength(15);
        client.Property(c => c.Address).HasMaxLength(150);
        client.Property(c => c.Bairro).HasMaxLength(100);
        client.Property(c => c.Complement).HasMaxLength(150);
      });
      builder.Entity<Client>()
.HasOne(dc => dc.Company)
.WithMany(c => c.Clients)
.HasForeignKey(dc => dc.IdCompany);
    }
    private void ConfiguraCompany(ModelBuilder builder)
    {
      builder.Entity<Company>(user =>
      {
        user.ToTable("tb_company");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Company>().HasData(new Company { Id = 1, CorporateName = "Empresa Padrão" });
    }
    private void ConfiguraFile(ModelBuilder builder)
    {
      builder.Entity<File>(user =>
      {
        user.ToTable("tb_file");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<File>()
    .HasOne(dc => dc.DescriptionFiles)
    .WithMany(c => c.Files)
    .HasForeignKey(dc => dc.IdDescriptionFiles);
    }

    private void ConfiguraDescriptionFiles(ModelBuilder builder)
    {
      builder.Entity<DescriptionFiles>(user =>
      {
        user.ToTable("tb_descriptionFiles");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<DescriptionFiles>()
   .HasOne(dc => dc.Company)
   .WithMany(c => c.DescriptionFiles)
   .HasForeignKey(dc => dc.idCompany);
    }

    private void ConfiguraProduct(ModelBuilder builder)
    {
      builder.Entity<Product>(user =>
      {
        user.ToTable("tb_product");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<Product>()
    .HasOne(dc => dc.Company)
    .WithMany(c => c.Products)
    .HasForeignKey(dc => dc.IdCompany);
    }
    private void ConfiguraService(ModelBuilder builder)
    {
      builder.Entity<ServiceProvided>(user =>
      {
        user.ToTable("tb_ServiceProvided");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<ServiceProvided>()
    .HasOne(dc => dc.Company)
    .WithMany(c => c.ServiceProvideds)
    .HasForeignKey(dc => dc.IdCompany);
    }

    private void ConfiguraBudget(ModelBuilder builder)
    {
      builder.Entity<Budget>(user =>
      {
        user.ToTable("tb_budget");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<Budget>()
    .HasOne(dc => dc.Company)
    .WithMany(c => c.Budgets)
    .HasForeignKey(dc => dc.IdCompany);
    }
    private void ConfiguraBudgetItems(ModelBuilder builder)
    {
      builder.Entity<BudgetItems>(user =>
      {
        user.ToTable("tb_budgetItems");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<BudgetItems>()
    .HasOne(dc => dc.Budget)
    .WithMany(c => c.BudgetItems)
    .HasForeignKey(dc => dc.IdBudget);
    }

    private void ConfiguraServiceProvision(ModelBuilder builder)
    {
      builder.Entity<ServicesProvision>(user =>
      {
        user.ToTable("tb_serviceProvision");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<ServicesProvision>()
    .HasOne(dc => dc.Company)
    .WithMany(c => c.ServiceProvisions)
    .HasForeignKey(dc => dc.IdCompany);

      builder.Entity<ServicesProvision>()
    .HasOne(dc => dc.Client)
    .WithMany(c => c.ServiceProvisions)
    .HasForeignKey(dc => dc.IdClient);

      builder.Entity<ServicesProvision>()
    .HasOne(dc => dc.Budget)
    .WithMany(c => c.ServiceProvisions)
    .HasForeignKey(dc => dc.IdBudget);
    }

    private void ConfiguraServiceProvisionItems(ModelBuilder builder)
    {
      builder.Entity<ServicesProvisionItems>(user =>
      {
        user.ToTable("tb_servicesProvisionItems");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();

      });
      builder.Entity<ServicesProvisionItems>()
    .HasOne(dc => dc.ServiceProvision)
    .WithMany(c => c.ServicesProvisionItems)
    .HasForeignKey(dc => dc.IdServiceProvision);
    }
    private void ConfiguraSalesman(ModelBuilder builder)
    {
      builder.Entity<Salesman>(user =>
      {
        user.ToTable("tb_salesman");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Salesman>()
      .HasOne(dc => dc.Company)
      .WithMany(c => c.Salesmen)
      .HasForeignKey(dc => dc.IdCompany);
    }
    private void ConfiguraSale(ModelBuilder builder)
    {
      builder.Entity<Sale>(user =>
      {
        user.ToTable("tb_sale");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Sale>()
   .HasOne(dc => dc.Company)
   .WithMany(c => c.Sale)
   .HasForeignKey(dc => dc.IdCompany);
      builder.Entity<Sale>()
  .HasOne(dc => dc.Client)
  .WithMany(c => c.Sale)
  .HasForeignKey(dc => dc.IdClient);
      builder.Entity<Sale>()
  .HasOne(dc => dc.Salesman)
  .WithMany(c => c.Sale)
  .HasForeignKey(dc => dc.IdSeller);
    }
    private void ConfiguraSaleItems(ModelBuilder builder)
    {
      builder.Entity<SaleItems>(user =>
      {
        user.ToTable("tb_saleItems");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<SaleItems>()
       .HasOne(dc => dc.Sale)
       .WithMany(c => c.SaleItems)
       .HasForeignKey(dc => dc.IdSale);
      builder.Entity<SaleItems>()
  .HasOne(dc => dc.Product)
  .WithMany(c => c.SaleItems)
  .HasForeignKey(dc => dc.IdProduct);
      builder.Entity<SaleItems>()
  .HasOne(dc => dc.ServiceProvided)
  .WithMany(c => c.SaleItems)
  .HasForeignKey(dc => dc.IdService);
    }
    private void ConfiguraCommission(ModelBuilder builder)
    {
      builder.Entity<Commission>(user =>
      {
        user.ToTable("tb_commission");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Commission>()
       .HasOne(dc => dc.Salesman)
       .WithMany(c => c.Commissions)
       .HasForeignKey(dc => dc.IdSalesman);
      builder.Entity<Commission>()
  .HasOne(dc => dc.Product)
  .WithMany(c => c.Commissions)
  .HasForeignKey(dc => dc.IdProduct);
      builder.Entity<Commission>()
  .HasOne(dc => dc.ServiceProvided)
  .WithMany(c => c.Commissions)
  .HasForeignKey(dc => dc.IdService);
      builder.Entity<Commission>()
.HasOne(dc => dc.CostCenter)
.WithMany(c => c.Commissions)
.HasForeignKey(dc => dc.IdCostCenter);
    }

    private void ConfiguraCostCenter(ModelBuilder builder)
    {
      builder.Entity<CostCenter>(user =>
      {
        user.ToTable("tb_costCenter");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<CostCenter>()
      .HasOne(dc => dc.Company)
      .WithMany(c => c.CostCenters)
      .HasForeignKey(dc => dc.IdCompany);
    }
    private void ConfiguraFinancial(ModelBuilder builder)
    {
      builder.Entity<Financial>(user =>
      {
        user.ToTable("tb_financial");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Financial>()
      .HasOne(dc => dc.Company)
      .WithMany(c => c.Financials)
      .HasForeignKey(dc => dc.IdCompany);
      builder.Entity<Financial>()
     .HasOne(dc => dc.CostCenter)
     .WithMany(c => c.Financials)
     .HasForeignKey(dc => dc.IdCostCenter);
      builder.Entity<Financial>()
      .HasOne(dc => dc.Salesman)
      .WithMany(c => c.Financials)
      .HasForeignKey(dc => dc.IdSalesman);
            builder.Entity<Financial>()
      .HasOne(dc => dc.Product)
      .WithMany(c => c.Financials)
      .HasForeignKey(dc => dc.IdProduct);
            builder.Entity<Financial>()
      .HasOne(dc => dc.ServiceProvided)
      .WithMany(c => c.Financials)
      .HasForeignKey(dc => dc.IdService);
            builder.Entity<Financial>()
      .HasOne(dc => dc.Sale)
      .WithMany(c => c.Financials)
      .HasForeignKey(dc => dc.IdSale);
      builder.Entity<Financial>()
      .HasOne(dc => dc.SaleItems)
      .WithMany(c => c.Financials)
      .HasForeignKey(dc => dc.IdSaleItems);
    }
    private void ConfiguraPlanCompany(ModelBuilder builder)
    {
      builder.Entity<PlanCompany>(user =>
      {
        user.ToTable("tb_planCompany");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<PlanCompany>()
     .HasOne(dc => dc.Company)
     .WithOne(c => c.PlanCompany)
     .HasForeignKey<PlanCompany>(c => c.IdCompany);
    }
    private void ConfiguraProspects(ModelBuilder builder)
    {
      builder.Entity<Prospects>(user =>
      {
        user.ToTable("tb_prospects");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Prospects>()
     .HasOne(dc => dc.Company)
     .WithMany(c => c.Prospects)
     .HasForeignKey(dc => dc.IdCompany);
    }
    private void ConfiguraPhasesProspects(ModelBuilder builder)
    {
      builder.Entity<PhasesProspects>(user =>
      {
        user.ToTable("tb_phasesProspects");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<PhasesProspects>()
     .HasOne(dc => dc.Prospects)
     .WithMany(c => c.PhasesProspects)
     .HasForeignKey(dc => dc.IdProspects);
    }
    private void ConfiguraSharedCommission(ModelBuilder builder)
    {
      builder.Entity<SharedCommission>(user =>
      {
        user.ToTable("tb_sharedCommission");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<SharedCommission>()
     .HasOne(dc => dc.SaleItems)
     .WithMany(c => c.SharedCommissions)
     .HasForeignKey(dc => dc.IdSaleItems);

      builder.Entity<SharedCommission>()
     .HasOne(dc => dc.Salesman)
     .WithMany(c => c.SharedCommissions)
     .HasForeignKey(dc => dc.IdSalesman);

      builder.Entity<SharedCommission>()
     .HasOne(dc => dc.CostCenter)
     .WithMany(c => c.SharedCommissions)
     .HasForeignKey(dc => dc.IdCostCenter);
    }
    private void ConfiguraClosuresDetail(ModelBuilder builder)
    {
      builder.Entity<ClosuresDetail>(user =>
      {
        user.ToTable("tb_closuresDetail");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<ClosuresDetail>()
     .HasOne(dc => dc.Closures)
     .WithMany(c => c.ClosuresDetails)
     .HasForeignKey(dc => dc.IdClosures);
    }
    private void ConfiguraClosures(ModelBuilder builder)
    {
      builder.Entity<Closures>(user =>
      {
        user.ToTable("tb_closures");
        user.HasKey(c => c.Id);
        user.Property(c => c.Id).ValueGeneratedOnAdd();
      });
      builder.Entity<Closures>()
     .HasOne(dc => dc.Salesman)
     .WithMany(c => c.Closures)
     .HasForeignKey(dc => dc.IdSalesman);
    }
  }
}
