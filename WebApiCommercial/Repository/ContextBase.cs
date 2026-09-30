using Microsoft.EntityFrameworkCore;
using Model;
using Model.Closure;
using Model.MDFe;
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
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<Purchase> Purchase { get; set; }
        public virtual DbSet<DriverLicense> DriverLicense { get; set; }
        public virtual DbSet<SituacaoTributaria> SituacaoTributaria { get; set; }
        public virtual DbSet<RegraFiscal> RegraFiscal { get; set; }
        public virtual DbSet<PurchaseItem> PurchaseItem { get; set; }
        public virtual DbSet<Vehicle> Vehicle { get; set; }
        public virtual DbSet<VehicleUsageHistory> VehicleUsageHistory { get; set; }

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
            ConfiguraSalePayment(modelBuilder);
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
            ConfiguraNFeEvento(modelBuilder);
            ConfiguraFinancialPaymentMethod(modelBuilder);
            ConfiguraProvider(modelBuilder);
            ConfiguraPurchase(modelBuilder);
            ConfiguraPurchaseItem(modelBuilder);
            ConfiguraSituacaoTributaria(modelBuilder);
            ConfiguraRegraFiscal(modelBuilder);

            ConfiguraServiceOrder(modelBuilder);
            ConfiguraServiceOrderItem(modelBuilder);
            ConfiguraServiceInvoice(modelBuilder);
            ConfiguraServiceInvoiceItem(modelBuilder);
            ConfiguraVehicle(modelBuilder);
            ConfiguraVehicleUsageHistory(modelBuilder);
            ConfiguraMdfe(modelBuilder);
            ConfiguraMdfeDocumento(modelBuilder);
            ConfiguraMdfePercurso(modelBuilder);
            ConfiguraMdfeVeiculo(modelBuilder);
            ConfiguraMdfeCondutor(modelBuilder);
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);
            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;


            base.OnModelCreating(modelBuilder);
        }

        private void ConfiguraFinancialPaymentMethod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialPaymentMethod>(entity =>
            {
                entity.ToTable("tb_financialPaymentMethod");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");

                // Configurar relacionamento com Financial
                entity.HasOne(e => e.Financial)
                    .WithMany(f => f.FinancialPaymentMethods)
                    .HasForeignKey(e => e.FinancialId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configurar relacionamento com PaymentMethod
                entity.HasOne(e => e.PaymentMethod)
                    .WithMany(p => p.FinancialPaymentMethods)
                    .HasForeignKey(e => e.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);

                // �ndice composto para evitar duplicidade
                entity.HasIndex(e => new { e.FinancialId, e.PaymentMethodId })
                    .IsUnique();
            });
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

//*************************************novas permiss�es***********************************************
//****************************************************************************************************
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
                user.Property(c => c.IsDeleted).HasDefaultValue(false);
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

                // RNTRC (ANTT) — usado pela RV10 do cadastro de veículos.
                //
                // 8 é o tamanho do CONTRATO, não uma escolha: o tipo TRNTRC dos
                // XSDs do MDF-e (mdfeModalRodoviario_v3.00.xsd) é
                // `<xs:pattern value="[0-9]{8}"/>`. Um valor maior que isso
                // passaria pelo banco e só seria recusado pela SEFAZ na emissão.
                client.Property(c => c.Rntrc).HasMaxLength(8);

                // Enum [Flags] persistido como integer (bitmask).
                client.Property(c => c.Profiles).HasConversion<int>();

                // Índice da FK IdCompany, declarado explicitamente para que o EF
                // não o remova ao detectar o índice composto abaixo.
                //
                // Isso importa: o índice único é PARCIAL (tem filtro), e o
                // Postgres só usa um índice parcial quando a consulta implica o
                // predicado. Ou seja, ele NÃO serve para as buscas que filtram
                // apenas por IdCompany — que são praticamente todas as consultas
                // de cliente do repositório.
                client.HasIndex(c => c.IdCompany);

                // RN01/RN11 — não duplicar pessoa na mesma empresa.
                //
                // O filtro é essencial: o cadastro simplificado grava
                // Document = "" e, sem ele, o segundo registro simplificado
                // seria rejeitado pelo índice.
                client.HasIndex(c => new { c.IdCompany, c.Document })
                    .IsUnique(false)
                    .HasFilter("\"Document\" <> ''")
                    .HasDatabaseName("IX_tb_client_IdCompany_Document");
            });
            builder.Entity<Client>()
                .HasOne(dc => dc.Company)
                .WithMany(c => c.Clients)
                .HasForeignKey(dc => dc.IdCompany);

            ConfiguraDriverLicense(builder);
        }

        private void ConfiguraDriverLicense(ModelBuilder builder)
        {
            builder.Entity<DriverLicense>(driverLicense =>
            {
                driverLicense.ToTable("tb_driver_license");
                driverLicense.HasKey(d => d.Id);
                driverLicense.Property(d => d.Id).ValueGeneratedOnAdd();

                // 1:1 com Client — o índice único em IdClient é o que garante
                // que um cliente não acumule duas CNHs.
                driverLicense.HasIndex(d => d.IdClient).IsUnique();

                // Restrict, e não Cascade: além de ser o comportamento desejado
                // aqui (a CNH não deve sumir por um delete acidental de cliente),
                // existe uma convenção global em OnModelCreating que rebaixa
                // todo FK Cascade para Restrict. Declarar explicitamente evita
                // que este código minta sobre o que acontece.
                driverLicense.HasOne(d => d.Client)
                    .WithOne(c => c.DriverLicense)
                    .HasForeignKey<DriverLicense>(d => d.IdClient)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// Cadastro de veículos (MDF-e). Segue a mesma estrutura de
        /// <see cref="ConfiguraClient"/>.
        /// </summary>
        private void ConfiguraVehicle(ModelBuilder builder)
        {
            builder.Entity<Vehicle>(vehicle =>
            {
                vehicle.ToTable("tb_vehicle");
                vehicle.HasKey(v => v.Id);
                vehicle.Property(v => v.Id).ValueGeneratedOnAdd();

                vehicle.Property(v => v.InternalCode).HasMaxLength(30);
                vehicle.Property(v => v.LicensePlate).HasMaxLength(8).IsRequired();
                vehicle.Property(v => v.Renavam).HasMaxLength(11);
                vehicle.Property(v => v.LicensingState).HasMaxLength(2).IsRequired();

                // Capacidade em M³ com precisão explícita: sem ela o Postgres
                // usa numeric sem escala fixa e o valor pode voltar com mais
                // casas decimais do que o MOC aceita (999.99).
                vehicle.Property(v => v.CapacityM3).HasPrecision(15, 2);

                // Datas SEM HasColumnType explícito, de propósito.
                //
                // O modelo de design time mapeia DateTime para "timestamp with
                // time zone", que é o tipo de 44 das 47 colunas de data do banco
                // — declarar aqui não mudaria o SQL gerado, só criaria a falsa
                // impressão de que é preciso. (As 3 exceções são as datas de CNH
                // em tb_driver_license, de outro módulo; ver
                // ConfiguraDriverLicense.)
                //
                // Cuidado ao investigar os AlterColumn de data que o scaffold
                // emite para OUTRAS tabelas: a causa é o snapshot
                // (ContextBaseModelSnapshot) ter ficado para trás, gravando
                // "timestamp without time zone" em colunas que no banco são
                // timestamptz. NÃO é o switch EnableLegacyTimestampBehavior do
                // Startup: o design time usa DesignTimeContextFactory
                // (IDesignTimeDbContextFactory) e nunca executa o Startup —
                // verificado por medição. Ver a migration AddVehicleMdfe.

                // Índice da FK declarado explicitamente para que o EF não o
                // remova ao detectar o índice composto abaixo (mesma razão do
                // ConfiguraClient).
                vehicle.HasIndex(v => v.IdCompany);
                vehicle.HasIndex(v => v.ClientId);

                // RV02 — placa única entre veículos ATIVOS da mesma empresa.
                //
                // O filtro é o que faz a exclusão lógica (RV11) liberar a placa:
                // sem ele, um veículo desativado continuaria bloqueando o
                // recadastro da mesma placa. Mesma técnica já usada em
                // tb_client, com a sintaxe de filtro do POSTGRES (aspas duplas
                // escapadas) — não a de SQL Server.
                vehicle.HasIndex(v => new { v.IdCompany, v.LicensePlate })
                    .IsUnique()
                    .HasFilter("\"IsActive\"")
                    .HasDatabaseName("IX_tb_vehicle_IdCompany_LicensePlate");

                // WithMany() sem parâmetro: Company não tem coleção de veículos,
                // e criá-la mexeria em um cadastro fora do escopo desta OS.
                vehicle.HasOne(v => v.Company)
                    .WithMany()
                    .HasForeignKey(v => v.IdCompany)
                    .OnDelete(DeleteBehavior.Restrict);

                // RV09/RV10 — proprietário terceiro (parceiro/transportador).
                vehicle.HasOne(v => v.Client)
                    .WithMany()
                    .HasForeignKey(v => v.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// RV14 — histórico de associação do veículo a MDF-e/OS. Tabela criada
        /// agora, populada quando a emissão existir.
        /// </summary>
        private void ConfiguraVehicleUsageHistory(ModelBuilder builder)
        {
            builder.Entity<VehicleUsageHistory>(history =>
            {
                history.ToTable("tb_vehicleUsageHistory");
                history.HasKey(h => h.Id);
                history.Property(h => h.Id).ValueGeneratedOnAdd();
                history.Property(h => h.Reference).HasMaxLength(200);

                history.HasIndex(h => h.IdVehicle);
                history.HasIndex(h => new { h.IdCompany, h.Source });

                history.HasOne(h => h.Vehicle)
                    .WithMany(v => v.UsageHistory)
                    .HasForeignKey(h => h.IdVehicle)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// MDF-e (modelo 58) — cabeçalho do manifesto. Ver <c>MdfeEmissao</c>.
        /// </summary>
        private void ConfiguraMdfe(ModelBuilder builder)
        {
            builder.Entity<MdfeEmissao>(mdfe =>
            {
                mdfe.ToTable("tb_mdfe");
                mdfe.HasKey(m => m.Id);
                mdfe.Property(m => m.Id).ValueGeneratedOnAdd();

                mdfe.Property(m => m.Serie).HasMaxLength(3).IsRequired();
                mdfe.Property(m => m.ChaveAcesso).HasMaxLength(44);
                mdfe.Property(m => m.Protocolo).HasMaxLength(20);
                mdfe.Property(m => m.UfCarregamento).HasMaxLength(2).IsRequired();
                mdfe.Property(m => m.UfDescarregamento).HasMaxLength(2).IsRequired();
                mdfe.Property(m => m.CodigoCIOT).HasMaxLength(12);

                // Aba "Carga/Produtos". Os limites são os do XSD, não escolha
                // nossa: xMunCarrega é 2..60 e xProd é 1..120, então truncar em
                // silêncio aqui viraria rejeição de schema lá na frente.
                mdfe.Property(m => m.CodMunCarregamento).HasMaxLength(7).IsRequired();
                mdfe.Property(m => m.MunCarregamento).HasMaxLength(60).IsRequired();
                mdfe.Property(m => m.ProdutoPredominante).HasMaxLength(120).IsRequired();

                // Aba "Informações Adicionais" (infAdFisco 2000 / infCpl 5000).
                mdfe.Property(m => m.InfoAdFisco).HasMaxLength(2000);
                mdfe.Property(m => m.InfoComplementar).HasMaxLength(5000);

                // Aba "Dados de Pagamento" (infPag.infBanc). Os limites são os do
                // XSD: codBanco 3..5, codAgencia 1..10, PIX 2..60. As três formas
                // são mutuamente exclusivas no leiaute (xs:choice) e nenhuma é
                // obrigatória — a exclusividade é conferida no MdfeBuilder, porque
                // não é expressável em constraint de coluna.
                mdfe.Property(m => m.PagamentoBanco).HasMaxLength(5);
                mdfe.Property(m => m.PagamentoAgencia).HasMaxLength(10);
                mdfe.Property(m => m.PagamentoCnpjIpef).HasMaxLength(14);
                mdfe.Property(m => m.PagamentoChavePix).HasMaxLength(60);

                // Dinheiro e peso com precisão explícita, pelo mesmo motivo do
                // CapacityM3 em ConfiguraVehicle: sem ela o Postgres usa numeric
                // sem escala fixa e o valor volta com mais casas do que o MOC
                // aceita. O peso do MDF-e é v3_3 (3 casas) no XSD.
                mdfe.Property(m => m.ValorTotal).HasPrecision(15, 2);
                mdfe.Property(m => m.PesoBruto).HasPrecision(15, 3);

                // Colunas text: um XML de manifesto passa de 8 KB com facilidade
                // (um grupo por município de descarga), e a resposta da SEFAZ
                // também não cabe em varchar curto.
                mdfe.Property(m => m.XmlCompleto).HasColumnType("text");
                mdfe.Property(m => m.ResponseJson).HasColumnType("text");
                mdfe.Property(m => m.ErrorMessage).HasColumnType("text");

                // Retorno da transmissão e eventos. Os limites de xMotivo e da
                // justificativa são os do leiaute (255); Recibo e
                // ProtocoloEncerramento acompanham o Protocolo (20), que já
                // existia.
                mdfe.Property(m => m.XMotivo).HasMaxLength(255);
                mdfe.Property(m => m.Recibo).HasMaxLength(20);
                mdfe.Property(m => m.ProtocoloEncerramento).HasMaxLength(20);
                mdfe.Property(m => m.JustificativaCancelamento).HasMaxLength(255);

                // Datas SEM HasColumnType explícito, como em ConfiguraVehicle —
                // o modelo de design time já mapeia para timestamptz, que é o
                // tipo predominante no banco.

                mdfe.HasIndex(m => m.IdCompany);
                mdfe.HasIndex(m => new { m.IdCompany, m.StatusMdfe });
                mdfe.HasIndex(m => m.IdVeiculoTracao);
                mdfe.HasIndex(m => m.ContratanteId);

                // Série + número únicos por empresa (RM09). O número é reservado
                // em transação com advisory lock no MdfeRepository, então este
                // índice não é o mecanismo — é a rede de segurança: se um dia
                // alguém criar manifesto por outro caminho, o banco recusa o
                // número repetido em vez de deixar dois documentos fiscais com a
                // mesma identidade.
                mdfe.HasIndex(m => new { m.IdCompany, m.Serie, m.Numero })
                    .IsUnique()
                    .HasDatabaseName("IX_tb_mdfe_IdCompany_Serie_Numero");

                // Chave de acesso única por empresa, mas SÓ quando preenchida: o
                // rascunho ainda não tem chave, e um índice único sem o filtro
                // bloquearia o segundo rascunho (todos teriam NULL, que em
                // Postgres não colide — o filtro deixa a intenção explícita em
                // vez de depender disso).
                mdfe.HasIndex(m => new { m.IdCompany, m.ChaveAcesso })
                    .IsUnique()
                    .HasFilter("\"ChaveAcesso\" IS NOT NULL")
                    .HasDatabaseName("IX_tb_mdfe_IdCompany_ChaveAcesso");

                mdfe.HasOne(m => m.Company)
                    .WithMany()
                    .HasForeignKey(m => m.IdCompany)
                    .OnDelete(DeleteBehavior.Restrict);

                // Comentário sobre a FK de tração: a relação é WithMany() sem
                // parâmetro porque Vehicle não tem coleção de manifestos — criá-la
                // mexeria no cadastro de veículo, fora do escopo. O histórico
                // (VehicleUsageHistory) é quem registra o uso.
                mdfe.HasOne(m => m.VeiculoTracao)
                    .WithMany()
                    .HasForeignKey(m => m.IdVeiculoTracao)
                    .OnDelete(DeleteBehavior.Restrict);

                mdfe.HasOne(m => m.Contratante)
                    .WithMany()
                    .HasForeignKey(m => m.ContratanteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// NF-e manifestadas. As duas origens (venda própria e compra de
        /// terceiro) são FKs anuláveis distintas — ver <c>MdfeDocumento</c>.
        /// </summary>
        private void ConfiguraMdfeDocumento(ModelBuilder builder)
        {
            builder.Entity<MdfeDocumento>(doc =>
            {
                doc.ToTable("tb_mdfeDocumento");
                doc.HasKey(d => d.Id);
                doc.Property(d => d.Id).ValueGeneratedOnAdd();

                doc.Property(d => d.ChaveNFe).HasMaxLength(44).IsRequired();
                doc.Property(d => d.Serie).HasMaxLength(3);
                doc.Property(d => d.PartnerName).HasMaxLength(60);
                doc.Property(d => d.UfOrigem).HasMaxLength(2);
                doc.Property(d => d.UfDestino).HasMaxLength(2);
                doc.Property(d => d.CodMunDescarga).HasMaxLength(7).IsRequired();
                doc.Property(d => d.MunicipioDescarga).HasMaxLength(60).IsRequired();

                doc.Property(d => d.ValorTotal).HasPrecision(15, 2);
                doc.Property(d => d.ValorMercadoria).HasPrecision(15, 2);
                doc.Property(d => d.PesoBruto).HasPrecision(15, 3);

                doc.HasIndex(d => d.IdMdfe);
                doc.HasIndex(d => d.NFeEmissionId);
                doc.HasIndex(d => d.PurchaseId);

                // RM10 — impede a MESMA chave de entrar duas vezes no mesmo
                // manifesto. Índice único por (manifesto, chave): a checagem de
                // "chave já usada em OUTRO manifesto" é feita no serviço, porque
                // depende do status do outro manifesto (cancelado libera) e isso
                // não cabe num índice.
                doc.HasIndex(d => new { d.IdMdfe, d.ChaveNFe })
                    .IsUnique()
                    .HasDatabaseName("IX_tb_mdfeDocumento_IdMdfe_ChaveNFe");

                doc.HasOne(d => d.Mdfe)
                    .WithMany(m => m.Documentos)
                    .HasForeignKey(d => d.IdMdfe)
                    .OnDelete(DeleteBehavior.Restrict);

                doc.HasOne(d => d.NFeEmission)
                    .WithMany()
                    .HasForeignKey(d => d.NFeEmissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                doc.HasOne(d => d.Purchase)
                    .WithMany()
                    .HasForeignKey(d => d.PurchaseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>UFs de percurso do manifesto (<c>infPercurso</c>) — RM13.</summary>
        private void ConfiguraMdfePercurso(ModelBuilder builder)
        {
            builder.Entity<MdfePercurso>(percurso =>
            {
                percurso.ToTable("tb_mdfePercurso");
                percurso.HasKey(p => p.Id);
                percurso.Property(p => p.Id).ValueGeneratedOnAdd();

                percurso.Property(p => p.UfPercurso).HasMaxLength(2).IsRequired();

                percurso.HasIndex(p => p.IdMdfe);

                percurso.HasOne(p => p.Mdfe)
                    .WithMany(m => m.Percurso)
                    .HasForeignKey(p => p.IdMdfe)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// Veículos escalados: 1 tração + 0..5 reboques. Placa, tara, rodado e
        /// carroceria NÃO são copiados — vêm do cadastro ao montar o XML.
        /// </summary>
        private void ConfiguraMdfeVeiculo(ModelBuilder builder)
        {
            builder.Entity<MdfeVeiculo>(veiculo =>
            {
                veiculo.ToTable("tb_mdfeVeiculo");
                veiculo.HasKey(v => v.Id);
                veiculo.Property(v => v.Id).ValueGeneratedOnAdd();

                veiculo.HasIndex(v => v.IdMdfe);
                veiculo.HasIndex(v => v.IdVehicle);

                // Um mesmo veículo não entra duas vezes na mesma escala. O limite
                // de "1 tração e no máximo 5 reboques" é validado no serviço
                // (RM05): é contagem por papel, e não caberia num índice.
                veiculo.HasIndex(v => new { v.IdMdfe, v.IdVehicle })
                    .IsUnique()
                    .HasDatabaseName("IX_tb_mdfeVeiculo_IdMdfe_IdVehicle");

                veiculo.HasOne(v => v.Mdfe)
                    .WithMany(m => m.Veiculos)
                    .HasForeignKey(v => v.IdMdfe)
                    .OnDelete(DeleteBehavior.Restrict);

                veiculo.HasOne(v => v.Vehicle)
                    .WithMany()
                    .HasForeignKey(v => v.IdVehicle)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        /// <summary>
        /// Condutores do manifesto. Nome e CPF são snapshot (o que foi declarado
        /// à fiscalização), então ficam gravados mesmo quando há IdClient.
        /// </summary>
        private void ConfiguraMdfeCondutor(ModelBuilder builder)
        {
            builder.Entity<MdfeCondutor>(condutor =>
            {
                condutor.ToTable("tb_mdfeCondutor");
                condutor.HasKey(c => c.Id);
                condutor.Property(c => c.Id).ValueGeneratedOnAdd();

                condutor.Property(c => c.Nome).HasMaxLength(60).IsRequired();
                condutor.Property(c => c.Cpf).HasMaxLength(11).IsRequired();

                condutor.HasIndex(c => c.IdMdfe);
                condutor.HasIndex(c => c.IdClient);

                // Mesmo CPF não pode aparecer duas vezes no mesmo manifesto —
                // é o documento que identifica o condutor na fiscalização.
                condutor.HasIndex(c => new { c.IdMdfe, c.Cpf })
                    .IsUnique()
                    .HasDatabaseName("IX_tb_mdfeCondutor_IdMdfe_Cpf");

                condutor.HasOne(c => c.Mdfe)
                    .WithMany(m => m.Condutores)
                    .HasForeignKey(c => c.IdMdfe)
                    .OnDelete(DeleteBehavior.Restrict);

                condutor.HasOne(c => c.Client)
                    .WithMany()
                    .HasForeignKey(c => c.IdClient)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfiguraCompany(ModelBuilder builder)
        {
            builder.Entity<Company>(user =>
            {
                user.ToTable("tb_company");
                user.HasKey(c => c.Id);
                user.Property(c => c.Id).ValueGeneratedOnAdd();
            });
            builder.Entity<Company>().HasData(new Company { Id = 1, CorporateName = "Empresa Padr�o" });
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
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("tb_product");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();

                // Tributacao por produto
                entity.Property(c => c.UsaTributacaoPropria).HasColumnName("UsaTributacaoPropria").IsRequired().HasDefaultValue(false);
                entity.Property(c => c.NaturezaOperacaoOrigemId).HasColumnName("NaturezaOperacaoOrigemId").IsRequired(false);
                entity.Property(c => c.DataAtualizacaoTributaria).HasColumnName("DataAtualizacaoTributaria").IsRequired(false);
                entity.Property(c => c.SituacaoTributariaId).HasColumnName("SituacaoTributariaId").IsRequired(false);
                entity.Property(c => c.PesoUnitario).HasColumnName("PesoUnitario").IsRequired(false).HasColumnType("numeric(18,4)");

                // Owned type ConfiguracaoTributaria
                entity.OwnsOne(e => e.ConfiguracaoTributaria, tb =>
                {
                    tb.Property(p => p.AplicarICMS).HasColumnName("Prod_AplicarICMS");
                    tb.Property(p => p.CstICMS).HasColumnName("Prod_CstICMS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaICMS).HasColumnName("Prod_AliquotaICMS").HasColumnType("decimal(18,4)");
                    tb.Property(p => p.ReduzirBaseICMS).HasColumnName("Prod_ReduzirBaseICMS");

                    tb.Property(p => p.AplicarIPI).HasColumnName("Prod_AplicarIPI");
                    tb.Property(p => p.CstIPI).HasColumnName("Prod_CstIPI").HasMaxLength(50);
                    tb.Property(p => p.AliquotaIPI).HasColumnName("Prod_AliquotaIPI").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarPIS).HasColumnName("Prod_AplicarPIS");
                    tb.Property(p => p.CstPIS).HasColumnName("Prod_CstPIS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaPIS).HasColumnName("Prod_AliquotaPIS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarCOFINS).HasColumnName("Prod_AplicarCOFINS");
                    tb.Property(p => p.CstCOFINS).HasColumnName("Prod_CstCOFINS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaCOFINS).HasColumnName("Prod_AliquotaCOFINS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarISSQN).HasColumnName("Prod_AplicarISSQN");
                    tb.Property(p => p.AliquotaISSQN).HasColumnName("Prod_AliquotaISSQN").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarIBS).HasColumnName("Prod_AplicarIBS");
                    tb.Property(p => p.CstIBS).HasColumnName("Prod_CstIBS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaIBS).HasColumnName("Prod_AliquotaIBS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarCBS).HasColumnName("Prod_AplicarCBS");
                    tb.Property(p => p.CstCBS).HasColumnName("Prod_CstCBS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaCBS).HasColumnName("Prod_AliquotaCBS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarIS).HasColumnName("Prod_AplicarIS");
                    tb.Property(p => p.AliquotaIS).HasColumnName("Prod_AliquotaIS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.cClassTrib).HasColumnName("Prod_cClassTrib").HasMaxLength(10);
                });
            });
            builder.Entity<Product>()
                .HasOne(dc => dc.Company)
                .WithMany(c => c.Products)
                .HasForeignKey(dc => dc.IdCompany);
            builder.Entity<Product>()
                .HasOne(dc => dc.SituacaoTributaria)
                .WithMany(c => c.Products)
                .HasForeignKey(dc => dc.SituacaoTributariaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }private void ConfiguraService(ModelBuilder builder)
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
                .HasForeignKey(dc => dc.BoxId);

            builder.Entity<Financial>()
                .HasOne(dc => dc.Client)
                .WithMany(c => c.Financials)
                .HasForeignKey(dc => dc.IdClient);

            builder.Entity<Financial>()
                .HasOne(dc => dc.BankAccount)
                .WithMany(c => c.Financials)
                .HasForeignKey(dc => dc.BankAccountId);
            builder.Entity<Financial>()
                .HasOne(dc => dc.Purchase)
                .WithMany(c => c.Financials)
                .HasForeignKey(dc => dc.IdPurchase)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            builder.Entity<Financial>()
                .HasOne(dc => dc.ServiceOrder)
                .WithMany(o => o.Financials)
                .HasForeignKey(dc => dc.IdServiceOrder)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
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
                entity.Property(e => e.PermiteTributacaoPorProduto).HasColumnName("PermiteTributacaoPorProduto").IsRequired().HasDefaultValue(false);

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

                    tb.Property(p => p.cClassTrib).HasColumnName("cClassTrib").HasMaxLength(10);
                });

                //entity.HasIndex(e => new { e.Cfop, e.TipoDocumento }).IsUnique();
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

                    // Série da DPS (NFS-e padrão Nacional)
                    nb.OwnsOne(n => n.Dps, dps =>
                    {
                        dps.Property(p => p.Serie).HasColumnName("Dps_Serie").HasMaxLength(50);
                        dps.Property(p => p.NumeroInicial).HasColumnName("Dps_NumeroInicial");
                    });

                    // Série e número do MDF-e (modelo 58) — RM09.
                    nb.OwnsOne(n => n.Mdfe, mdfe =>
                    {
                        mdfe.Property(p => p.Serie).HasColumnName("Mdfe_Serie").HasMaxLength(50);
                        mdfe.Property(p => p.NumeroInicial).HasColumnName("Mdfe_NumeroInicial");
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
                    em.Property(p => p.InscricaoMunicipal).HasColumnName("Emitente_InscricaoMunicipal").HasMaxLength(30);
                    // RNTRC do emitente (RM08). 8 dígitos é o tipo TRNTRC dos XSDs
                    // do MDF-e ([0-9]{8}), o mesmo limite da coluna em tb_client —
                    // deixar maior permitiria gravar um RNTRC que só seria recusado
                    // na SEFAZ.
                    em.Property(p => p.Rntrc).HasColumnName("Emitente_Rntrc").HasMaxLength(8);
                    em.Property(p => p.RazaoSocial).HasColumnName("Emitente_RazaoSocial").HasMaxLength(250);
                    em.Property(p => p.Fantasia).HasColumnName("Emitente_Fantasia").HasMaxLength(250);
                    em.Property(p => p.Logo).HasColumnName("Emitente_Logo").HasColumnType("bytea");

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
                        rt.Property(r => r.OpcaoSimplesNacional).HasColumnName("Emitente_OpcaoSimplesNacional");
                    });
                });

                entity.Property(e => e.AutorizacaoASO).HasColumnName("AutorizacaoASO");

                // NFSe fields
                entity.Property(e => e.LastInvoiceNumber).HasColumnName("LastInvoiceNumber");
                entity.Property(e => e.CodMunIBGE).HasColumnName("CodMunIBGE").HasMaxLength(7);
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

                entity.HasOne(e => e.Company)
                    .WithMany(e => e.NFeEmissions)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NaturezaOperacao)
                    .WithMany(e => e.NFeEmissions)
                    .HasForeignKey(e => e.NaturezaOperacaoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Sale)
                    .WithMany(e => e.NFeEmissions)
                    .HasForeignKey(e => e.SaleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfiguraNFeEvento(ModelBuilder builder)
        {
            builder.Entity<NFeEvento>(entity =>
            {
                entity.ToTable("tb_nfeEvento");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.DescricaoEvento).HasMaxLength(200);
                entity.Property(e => e.ChaveAcesso).HasMaxLength(44);
                entity.Property(e => e.Protocolo).HasMaxLength(15);
                entity.Property(e => e.Correcao).HasColumnType("text");
                entity.Property(e => e.XMotivo).HasColumnType("text");
                entity.Property(e => e.XmlEvento).HasColumnType("text");

                entity.Property(e => e.TipoEvento);
                entity.Property(e => e.NSeqEvento);
                entity.Property(e => e.CStat);
                // Padrão do banco (todas as demais tabelas usam timestamp with time zone)
                entity.Property(e => e.DhRegEvento).HasColumnType("timestamp with time zone");
                entity.Property(e => e.Situacao);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");

                entity.HasOne(e => e.NFeEmission)
                    .WithMany()
                    .HasForeignKey(e => e.NFeEmissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.NFeEmissionId, e.TipoEvento, e.Situacao });
            });
        }

        // =====================================================================================
        // Ordem de serviço e NFS-e (padrão Nacional)
        //
        // Duas particularidades desta família de tabelas:
        //
        // 1. ServiceOrder/ServiceInvoice têm "int TenantId" + navigation "Company", mas NÃO têm
        //    CompanyId. A FK é o próprio TenantId — por isso o HasForeignKey aponta para ele.
        //
        // 2. O loop global em OnModelCreating reescreve toda FK Cascade para Restrict DEPOIS de
        //    todos os Configura*. Escrever Cascade aqui seria no-op silencioso; o Restrict é
        //    explícito para que a intenção fique legível.
        // =====================================================================================

        private void ConfiguraServiceOrder(ModelBuilder builder)
        {
            builder.Entity<ServiceOrder>(entity =>
            {
                entity.ToTable("tb_serviceOrder");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.OrderDate).HasColumnType("timestamp with time zone");
                entity.Property(e => e.Competence).HasColumnType("timestamp with time zone");
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.ConcludedAt).HasColumnType("timestamp with time zone");

                entity.Property(e => e.Notes).HasColumnType("text");
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);

                entity.Property(e => e.TotalValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IssqnValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IssqnRetidoValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RetentionValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetValue).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.OrderDate);
            });
        }

        private void ConfiguraServiceOrderItem(ModelBuilder builder)
        {
            builder.Entity<ServiceOrderItem>(entity =>
            {
                entity.ToTable("tb_serviceOrderItem");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.Description);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                // Alíquotas em 4 casas: a casa já guarda AliquotaICMS/AliquotaISSQN assim,
                // e há alíquota de ISS com 4 decimais (ex.: 2,0000 / 4,5000).
                entity.Property(e => e.IssqnRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.PisRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CofinsRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.IrRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CsllRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.InssRate).HasColumnType("decimal(18,4)");

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(o => o.ServiceOrderItems)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ServiceProvided)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceProvidedId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ServiceOrderId);
            });
        }

        private void ConfiguraServiceInvoice(ModelBuilder builder)
        {
            builder.Entity<ServiceInvoice>(entity =>
            {
                entity.ToTable("tb_serviceInvoice");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.DhEmissao).HasColumnType("timestamp with time zone");
                entity.Property(e => e.DataCompetencia).HasColumnType("timestamp with time zone");
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.EmittedAt).HasColumnType("timestamp with time zone");

                // ChaveAcesso: 50 dígitos. IdDPS: 45 caracteres. Ambos ficam vazios/nulos
                // enquanto a fatura está Pendente — daí o índice ser COMUM e nunca único.
                entity.Property(e => e.ChaveAcesso);
                entity.Property(e => e.IdDPS);
                entity.Property(e => e.Protocolo);
                entity.Property(e => e.CodMunIBGE);
                entity.Property(e => e.CancelReason);

                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.TipoAmbiente).HasConversion<string>();

                entity.Property(e => e.TotalValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IssqnValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IssqnRetidoValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RetentionValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetValue).HasColumnType("decimal(18,2)");

                // Diagnóstico da transmissão — mesmo tratamento de tb_nfeEmission.
                entity.Property(e => e.XmlNfse).HasColumnType("text");
                entity.Property(e => e.RequestPayloadJson).HasColumnType("text");
                entity.Property(e => e.ResponseJson).HasColumnType("text");
                entity.Property(e => e.ErrorMessage).HasColumnType("text");

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(o => o.ServiceInvoices)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.ServiceOrderId);
                entity.HasIndex(e => e.ChaveAcesso);
                entity.HasIndex(e => e.IdDPS);
            });
        }

        private void ConfiguraServiceInvoiceItem(ModelBuilder builder)
        {
            builder.Entity<ServiceInvoiceItem>(entity =>
            {
                entity.ToTable("tb_serviceInvoiceItem");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.Property(e => e.IssqnRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.PisRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CofinsRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.IrRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CsllRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.InssRate).HasColumnType("decimal(18,4)");

                entity.HasOne(e => e.ServiceInvoice)
                    .WithMany(i => i.ServiceInvoiceItems)
                    .HasForeignKey(e => e.ServiceInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ServiceProvided)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceProvidedId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ServiceInvoiceId);
            });
        }

        private void ConfiguraProvider(ModelBuilder builder)
        {
            builder.Entity<Provider>(entity =>
            {
                entity.ToTable("tb_provider");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.nome).HasMaxLength(200);
                entity.Property(c => c.razaoSocial).HasMaxLength(200);
                entity.Property(c => c.nomeFantasia).HasMaxLength(200);
                entity.Property(c => c.cnpj).HasMaxLength(20);
                entity.Property(c => c.inscricaoEstadual).HasMaxLength(20);
                entity.Property(c => c.telefone).HasMaxLength(20);
                entity.Property(c => c.email).HasMaxLength(100);
                entity.Property(c => c.logradouro).HasMaxLength(200);
                entity.Property(c => c.bairro).HasMaxLength(100);
                entity.Property(c => c.cidade).HasMaxLength(100);
                entity.Property(c => c.uf).HasMaxLength(2);
                entity.Property(c => c.cep).HasMaxLength(10);
                entity.Property(c => c.complemento).HasMaxLength(200);
            });
            builder.Entity<Provider>()
                .HasOne(dc => dc.Company)
                .WithMany(c => c.Providers)
                .HasForeignKey(dc => dc.IdCompany);
        }

        private void ConfiguraPurchase(ModelBuilder builder)
        {
            builder.Entity<Purchase>(entity =>
            {
                entity.ToTable("tb_purchase");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.ChaveNfe).HasMaxLength(44).IsRequired();
                entity.Property(c => c.ValorTotal).HasColumnType("decimal(18,2)");

                // Detalhamento de custos/impostos (nullable)
                entity.Property(c => c.ValorProdutos).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorFrete).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorSeguro).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorDesconto).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorIPI).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorPIS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorCOFINS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorICMS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorIBS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorCBS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.BaseCalculoICMS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.BaseCalculoIBSCBS).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorNotaFiscal).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorTotalTributos).HasColumnType("decimal(18,2)");
                entity.Property(c => c.CustosExtrasJson).HasColumnType("text").IsRequired(false);
                entity.Property(c => c.Observacao).HasColumnType("text").IsRequired(false);
            });
            builder.Entity<Purchase>()
                .HasOne(dc => dc.Company)
                .WithMany(c => c.Purchases)
                .HasForeignKey(dc => dc.IdCompany)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Purchase>()
                .HasOne(dc => dc.Fornecedor)
                .WithMany(c => c.Purchases)
                .HasForeignKey(dc => dc.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        private void ConfiguraPurchaseItem(ModelBuilder builder)
        {
            builder.Entity<PurchaseItem>(entity =>
            {
                entity.ToTable("tb_purchaseItem");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.CodigoProduto).HasMaxLength(50);
                entity.Property(c => c.DescricaoProduto).HasMaxLength(200);
                entity.Property(c => c.Quantidade).HasColumnType("decimal(18,3)");
                entity.Property(c => c.ValorUnitario).HasColumnType("decimal(18,4)");
                entity.Property(c => c.Desconto).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ValorTotal).HasColumnType("decimal(18,2)");

                // Fator de conversao (origem da linha no XML)
                entity.Property(c => c.Unidade).HasMaxLength(6).IsRequired(false);
                entity.Property(c => c.QuantidadeXml).HasColumnType("decimal(18,3)").IsRequired(false);
                entity.Property(c => c.ValorUnitarioXml).HasColumnType("decimal(18,4)").IsRequired(false);
                entity.Property(c => c.FatorConversao).HasColumnType("decimal(18,6)").IsRequired(false);
            });
            builder.Entity<PurchaseItem>()
                .HasOne(dc => dc.Compra)
                .WithMany(c => c.PurchaseItems)
                .HasForeignKey(dc => dc.CompraId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PurchaseItem>()
                .HasOne(dc => dc.Produto)
                .WithMany(c => c.PurchaseItems)
                .HasForeignKey(dc => dc.ProdutoId);
        }

        private void ConfiguraSituacaoTributaria(ModelBuilder builder)
        {
            builder.Entity<SituacaoTributaria>(entity =>
            {
                entity.ToTable("tb_situacaoTributaria");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Codigo).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(150).IsRequired();

                entity.HasIndex(e => new { e.CompanyId, e.Codigo }).IsUnique();
            });
            builder.Entity<SituacaoTributaria>()
                .HasOne(dc => dc.Company)
                .WithMany()
                .HasForeignKey(dc => dc.CompanyId);
        }

        private void ConfiguraRegraFiscal(ModelBuilder builder)
        {
            builder.Entity<RegraFiscal>(entity =>
            {
                entity.ToTable("tb_regraFiscal");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Cfop).HasMaxLength(10).IsRequired();

                entity.Property(e => e.Destino)
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .IsRequired();

                entity.OwnsOne(e => e.ConfiguracaoTributaria, tb =>
                {
                    tb.Property(p => p.AplicarICMS).HasColumnName("RF_AplicarICMS");
                    tb.Property(p => p.CstICMS).HasColumnName("RF_CstICMS").HasMaxLength(50);
                    tb.Property(p => p.CsosnICMS).HasColumnName("RF_CsosnICMS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaICMS).HasColumnName("RF_AliquotaICMS").HasColumnType("decimal(18,4)");
                    tb.Property(p => p.ReduzirBaseICMS).HasColumnName("RF_ReduzirBaseICMS");

                    tb.Property(p => p.AplicarIPI).HasColumnName("RF_AplicarIPI");
                    tb.Property(p => p.CstIPI).HasColumnName("RF_CstIPI").HasMaxLength(50);
                    tb.Property(p => p.AliquotaIPI).HasColumnName("RF_AliquotaIPI").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarPIS).HasColumnName("RF_AplicarPIS");
                    tb.Property(p => p.CstPIS).HasColumnName("RF_CstPIS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaPIS).HasColumnName("RF_AliquotaPIS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarCOFINS).HasColumnName("RF_AplicarCOFINS");
                    tb.Property(p => p.CstCOFINS).HasColumnName("RF_CstCOFINS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaCOFINS).HasColumnName("RF_AliquotaCOFINS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarISSQN).HasColumnName("RF_AplicarISSQN");
                    tb.Property(p => p.AliquotaISSQN).HasColumnName("RF_AliquotaISSQN").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarIBS).HasColumnName("RF_AplicarIBS");
                    tb.Property(p => p.CstIBS).HasColumnName("RF_CstIBS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaIBS).HasColumnName("RF_AliquotaIBS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarCBS).HasColumnName("RF_AplicarCBS");
                    tb.Property(p => p.CstCBS).HasColumnName("RF_CstCBS").HasMaxLength(50);
                    tb.Property(p => p.AliquotaCBS).HasColumnName("RF_AliquotaCBS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.AplicarIS).HasColumnName("RF_AplicarIS");
                    tb.Property(p => p.AliquotaIS).HasColumnName("RF_AliquotaIS").HasColumnType("decimal(18,4)");

                    tb.Property(p => p.cClassTrib).HasColumnName("RF_cClassTrib").HasMaxLength(10);
                });

                entity.HasIndex(e => new { e.NaturezaOperacaoId, e.SituacaoTributariaId, e.Destino }).IsUnique();
            });

            builder.Entity<RegraFiscal>()
                .HasOne(e => e.NaturezaOperacao)
                .WithMany(n => n.RegrasFiscais)
                .HasForeignKey(e => e.NaturezaOperacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RegraFiscal>()
                .HasOne(e => e.SituacaoTributaria)
                .WithMany(s => s.RegrasFiscais)
                .HasForeignKey(e => e.SituacaoTributariaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        private void ConfiguraSalePayment(ModelBuilder builder)
        {
            builder.Entity<SalePayment>(entity =>
            {
                entity.ToTable("tb_salePayment");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Sale)
                    .WithMany(s => s.SalePayments)
                    .HasForeignKey(e => e.IdSale)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}