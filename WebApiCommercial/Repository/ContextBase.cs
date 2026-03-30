using Microsoft.EntityFrameworkCore;
using Model;
using Model.Closure;
using Model.Moves;
using Model.Registrations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            NormalizeEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }
        private void NormalizeEntities()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var entity in entries)
            {
                NormalizationHelper.NormalizeEntity(entity);
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseSqlServer(@"Server=.\sqlexpress;Database=serviceboxdb;Trusted_Connection=True;");
            //}
            //if (!optionsBuilder.IsConfigured)
            //{
            //    // Pega a connection string do appsettings.json
            //    var configuration = new ConfigurationBuilder()
            //        .SetBasePath(Directory.GetCurrentDirectory())
            //        .AddJsonFile("appsettings.json")
            //        .Build();

            //    var connectionString = configuration.GetConnectionString("PostgreConnection");

            //    optionsBuilder.UseNpgsql(connectionString);
            //}
            //optionsBuilder.AddInterceptors(new CaseInsensitiveQueryInterceptor());
            //optionsBuilder.AddInterceptors(new TenantQueryInterceptor(_tenantProvider));
            //base.OnConfiguring(optionsBuilder);
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
            ConfiguraDetailsService(modelBuilder);
            ConfiguraStockService(modelBuilder);
            ConfiguraBox(modelBuilder);
            ConfiguraFinancialResources(modelBuilder);
            ConfiguraPaymentMethod(modelBuilder);
            ConfiguraPermission(modelBuilder);
            ConfiguraUserPermission(modelBuilder);
            ConfiguraBankAccount(modelBuilder);
            ConfiguraNaturezaOperacao(modelBuilder);
            ConfiguraFiscalConfiguration(modelBuilder);
            ConfiguraNFeEmission(modelBuilder);
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
     .SelectMany(t => t.GetForeignKeys())
     .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);
            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;


            base.OnModelCreating(modelBuilder);
        }

        private void ConfiguraBankAccount(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankAccount>(d =>
            {
                d.ToTable("tb_bankAccount");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
            modelBuilder.Entity<BankAccount>()
            .HasOne(dc => dc.Company)
            .WithMany(c => c.BankAccounts)
            .HasForeignKey(dc => dc.IdCompany);
        }

        private void ConfiguraUserPermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPermission>(d =>
            {
                d.ToTable("tb_userPermission");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
            modelBuilder.Entity<UserPermission>()
            .HasOne(dc => dc.User)
            .WithMany(c => c.UserPermissions)
            .HasForeignKey(dc => dc.UserId);
            modelBuilder.Entity<UserPermission>()
.HasOne(dc => dc.Permission)
.WithMany(c => c.UserPermissions)
.HasForeignKey(dc => dc.PermissionId);


        }

        private void ConfiguraPermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(d =>
            {
                d.ToTable("tb_permission");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedNever().IsRequired();
                d.HasIndex(c => c.Code)
           .IsUnique();
                d.HasData(SeedPermissions.GetDefaultPermissions());
            });

        }

        private void ConfiguraPaymentMethod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethod>(d =>
            {
                d.ToTable("tb_paymentMethod");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
            modelBuilder.Entity<PaymentMethod>()
   .HasOne(dc => dc.Company)
   .WithMany(c => c.PaymentMethods)
   .HasForeignKey(dc => dc.IdCompany);

        }

        private void ConfiguraFinancialResources(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialResources>(d =>
            {
                d.ToTable("tb_financialResources");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
        }

        private void ConfiguraBox(ModelBuilder builder)
        {
            builder.Entity<Box>(d =>
            {
                d.ToTable("tb_box");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
            builder.Entity<Box>()
     .HasOne(dc => dc.Company)
     .WithMany(c => c.Boxes)
     .HasForeignKey(dc => dc.IdCompany);
        }

        private void ConfiguraStockService(ModelBuilder builder)
        {
            builder.Entity<Stock>(d =>
            {
                d.ToTable("tb_stock");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
            builder.Entity<Stock>()
              .HasOne(dc => dc.Product)
              .WithMany(c => c.Stocks)
              .HasForeignKey(dc => dc.IdProduct);

            builder.Entity<Stock>()
     .HasOne(dc => dc.Company)
     .WithMany(c => c.Stocks)
     .HasForeignKey(dc => dc.IdCompany);
        }

        private void ConfiguraDetailsService(ModelBuilder builder)
        {
            builder.Entity<DetailsService>(d =>
            {
                d.ToTable("tb_detailsService");
                d.HasKey(c => c.Id);
                d.Property(c => c.Id).ValueGeneratedOnAdd();

            });
            builder.Entity<DetailsService>()
              .HasOne(dc => dc.ServiceProvided)
              .WithMany(c => c.Details)
              .HasForeignKey(dc => dc.IdServiceProvided);
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
                client.Property(c => c.Email).HasMaxLength(100);
                client.Property(c => c.Bairro).HasMaxLength(100);
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

            builder.Entity<Financial>()
        .HasOne(dc => dc.Box)
        .WithMany(c => c.Movimentacoes)
        .HasForeignKey(dc => dc.IdProduct);

            builder.Entity<Financial>()
        .HasOne(dc => dc.Client)
        .WithMany(c => c.Financials)
        .HasForeignKey(dc => dc.IdClient);
            builder.Entity<Financial>()
         .HasOne(dc => dc.PaymentMethod)
         .WithMany(c => c.Financials)
         .HasForeignKey(dc => dc.PaymentMethodId);
            builder.Entity<Financial>()
         .HasOne(dc => dc.BankAccount)
         .WithMany(c => c.Financials)
         .HasForeignKey(dc => dc.BankAccountId);
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

        private void ConfiguraNaturezaOperacao(ModelBuilder builder)
        {
            builder.Entity<NaturezaOperacao>(entity =>
            {
                entity.ToTable("tb_naturezaOperacao");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Descricao).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Cfop).HasMaxLength(10).IsRequired();

                // Enums como string
                entity.Property(e => e.TipoDocumento).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.Finalidade).HasConversion<string>().HasMaxLength(50).IsRequired();

                entity.Property(e => e.ConsumidorFinal).IsRequired();
                entity.Property(e => e.MovimentaEstoque).IsRequired();
                entity.Property(e => e.Ativo).IsRequired();

                // Owned type configuracaoTributaria (colunas na mesma tabela)
                entity.OwnsOne(e => e.ConfiguracaoTributaria, tb =>
                {
                    tb.Property(p => p.AplicarICMS).HasColumnName("AplicarICMS");
                    tb.Property(p => p.CstICMS).HasColumnName("CstICMS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaICMS).HasColumnName("AliquotaICMS").HasColumnType("decimal(18,4)");
                    tb.Property(p => p.ReduzirBaseICMS).HasColumnName("ReduzirBaseICMS");

                    tb.Property(p => p.AplicarIPI).HasColumnName("AplicarIPI");
                    tb.Property(p => p.CstIPI).HasColumnName("CstIPI").HasMaxLength(50);
                    tb.Property(p => p.AliquotaIPI).HasColumnName("AliquotaIPI").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarPIS).HasColumnName("AplicarPIS");
                    tb.Property(p => p.CstPIS).HasColumnName("CstPIS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaPIS).HasColumnName("AliquotaPIS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarCOFINS).HasColumnName("AplicarCOFINS");
                    tb.Property(p => p.CstCOFINS).HasColumnName("CstCOFINS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaCOFINS).HasColumnName("AliquotaCOFINS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarISSQN).HasColumnName("AplicarISSQN");
                    tb.Property(p => p.AliquotaISSQN).HasColumnName("AliquotaISSQN").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarIBS).HasColumnName("AplicarIBS");
                    tb.Property(p => p.CstIBS).HasColumnName("CstIBS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaIBS).HasColumnName("AliquotaIBS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarCBS).HasColumnName("AplicarCBS");
                    tb.Property(p => p.CstCBS).HasColumnName("CstCBS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaCBS).HasColumnName("AliquotaCBS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarIS).HasColumnName("AplicarIS");
                    tb.Property(p => p.AliquotaIS).HasColumnName("AliquotaIS").HasColumnType("decimal(18,4)");
                });

                entity.HasIndex(e => new { e.Cfop, e.TipoDocumento }).IsUnique();
            });
            builder.Entity<NaturezaOperacao>()
               .HasOne(dc => dc.Company)
               .WithMany(c => c.NaturezaOperacoes)
               .HasForeignKey(dc => dc.CompanyId);
        }
     
        private void ConfiguraFiscalConfiguration(ModelBuilder builder)
        {
            builder.Entity<Model.Registrations.FiscalConfiguration>(entity =>
            {
                entity.ToTable("tb_fiscalConfiguration");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                // Ambiente como string
                entity.Property(e => e.Ambiente).HasConversion<string>().HasMaxLength(50);

                // NumeracaoDocumentos (owned)
                entity.OwnsOne(e => e.NumeracaoDocumentos, nb =>
                {
                    nb.OwnsOne(n => n.Nfe, nfe =>
                    {
                        nfe.Property(p => p.Serie).HasColumnName("Nfe_Serie").HasMaxLength(50);
                        nfe.Property(p => p.NumeroInicial).HasColumnName("Nfe_NumeroInicial");
                    });

                    nb.OwnsOne(n => n.Nfce, nfce =>
                    {
                        nfce.Property(p => p.Serie).HasColumnName("Nfce_Serie").HasMaxLength(50);
                        nfce.Property(p => p.NumeroInicial).HasColumnName("Nfce_NumeroInicial");
                    });
                });

                // CertificadoDigital (owned)
                entity.OwnsOne(e => e.CertificadoDigital, cb =>
                {
                    cb.Property(p => p.Arquivo).HasColumnName("Certificado_Arquivo").HasMaxLength(2000);
                    cb.Property(p => p.Senha).HasColumnName("Certificado_Senha").HasMaxLength(200);
                });

                // CSC (owned)
                entity.OwnsOne(e => e.Csc, c =>
                {
                    c.Property(p => p.Identificador).HasColumnName("Csc_Identificador").HasMaxLength(200);
                    c.Property(p => p.Valor).HasColumnName("Csc_Valor").HasMaxLength(500);
                });

                // Emitente (owned)
                entity.OwnsOne(e => e.Emitente, em =>
                {
                    em.Property(p => p.Cnpj).HasColumnName("Emitente_Cnpj").HasMaxLength(20);
                    em.Property(p => p.Cpf).HasColumnName("Emitente_Cpf").HasMaxLength(20);
                    em.Property(p => p.InscricaoEstadual).HasColumnName("Emitente_InscricaoEstadual").HasMaxLength(100);
                    em.Property(p => p.RazaoSocial).HasColumnName("Emitente_RazaoSocial").HasMaxLength(250);
                    em.Property(p => p.Fantasia).HasColumnName("Emitente_Fantasia").HasMaxLength(250);

                    em.OwnsOne(p => p.EmitenteContato, ct =>
                    {
                        ct.Property(c => c.Telefone).HasColumnName("Emitente_Telefone").HasMaxLength(50);
                    });

                    em.OwnsOne(p => p.EmitenteEndereco, ed =>
                    {
                        ed.Property(ea => ea.Cep).HasColumnName("Emitente_Cep").HasMaxLength(20);
                        ed.Property(ea => ea.Logradouro).HasColumnName("Emitente_Logradouro").HasMaxLength(250);
                        ed.Property(ea => ea.Numero).HasColumnName("Emitente_Numero").HasMaxLength(50);
                        ed.Property(ea => ea.Complemento).HasColumnName("Emitente_Complemento").HasMaxLength(200);
                        ed.Property(ea => ea.Bairro).HasColumnName("Emitente_Bairro").HasMaxLength(100);
                        ed.Property(ea => ea.CodigoCidade).HasColumnName("Emitente_CodigoCidade").HasMaxLength(50);
                        ed.Property(ea => ea.Cidade).HasColumnName("Emitente_Cidade").HasMaxLength(150);
                        ed.Property(ea => ea.Uf).HasColumnName("Emitente_Uf").HasMaxLength(10);
                    });

                    em.OwnsOne(p => p.RegimeTributario, rt =>
                    {
                        rt.Property(r => r.Crt).HasColumnName("Emitente_Crt").HasMaxLength(10);
                    });
                });

                entity.Property(e => e.AutorizacaoASO).HasColumnName("AutorizacaoASO");
            });
            builder.Entity<FiscalConfiguration>()
             .HasOne(dc => dc.Company)
             .WithOne(c => c.FiscalConfiguration)
             .HasForeignKey<FiscalConfiguration>(dc => dc.CompanyId);
        }
        private void ConfiguraNFeEmission(ModelBuilder builder)
        {
            builder.Entity<NFeEmission>(entity =>
            {
                entity.ToTable("tb_nfeEmission");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Serie).HasMaxLength(50);
                entity.Property(e => e.Numero);

                entity.Property(e => e.RequestPayloadJson).HasColumnType("text");
                entity.Property(e => e.ResponseJson).HasColumnType("text");
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);

                entity.HasOne(e=>e.Company)
                    .WithMany(e => e.NFeEmissions)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e=>e.NaturezaOperacao)
                    .WithMany(e=>e.NFeEmissions)
                    .HasForeignKey(e => e.NaturezaOperacaoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e=>e.Sale)
                    .WithMany(e => e.NFeEmissions)
                    .HasForeignKey(e => e.SaleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
