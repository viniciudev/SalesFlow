using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Model;
using Model.Closure;
using Model.Moves;
using Model.Registrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProfControl.WebApi;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using Settings = ProfControl.WebApi.Settings;

namespace WebAppCommercial
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers().AddNewtonsoftJson(options =>
      {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
      });
      services.AddDbContext<ContextBase>(options =>
           options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

      services.Configure<GzipCompressionProviderOptions>(options =>
      {
        options.Level = CompressionLevel.Optimal;
      });

      services.AddResponseCompression(options =>
      {
        IEnumerable<string> MimeTypes = new[]
        {
         // General
         "text/plain",
         "text/html",
         "text/css",
         "font/woff2",
         "application/javascript",
         "image/x-icon",
         "image/png"
        };

        options.EnableForHttps = true;
        options.MimeTypes = MimeTypes;
        options.Providers.Add<GzipCompressionProvider>();
      });

      #region Registions
      //Usuário
      services.AddTransient<IUserService, UserService>();
      services.AddTransient<IGenericRepository<User>, UserRepository>();
      services.AddTransient<IUserRepository, UserRepository>();
      services.AddTransient<IBaseService<User>, UserService>();
      //Client
      services.AddTransient<IClientService, ClientService>();
      services.AddTransient<IGenericRepository<Client>, ClientRepository>();
      services.AddTransient<IClientRepository, ClientRepository>();
      services.AddTransient<IBaseService<Client>, ClientService>();
      //File
      services.AddTransient<IFileService, FileService>();
      services.AddTransient<IGenericRepository<File>, FileRepository>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddTransient<IBaseService<File>, FileService>();
      //company
      services.AddTransient<ICompanyService, CompanyService>();
      services.AddTransient<IGenericRepository<Company>, CompanyRepository>();
      services.AddTransient<ICompanyRepository, CompanyRepository>();
      services.AddTransient<IBaseService<Company>, CompanyService>();
      //DescriptionFiles
      services.AddTransient<IDescriptionFilesService, DescriptionFilesService>();
      services.AddTransient<IGenericRepository<DescriptionFiles>, DescriptionFilesRepository>();
      services.AddTransient<IDescriptionFilesRepository, DescriptionFilesRepository>();
      services.AddTransient<IBaseService<DescriptionFiles>, DescriptionFilesService>();
      //PlanCompany
      services.AddTransient<IPlanCompanyService, PlanCompanyService>();
      services.AddTransient<IGenericRepository<PlanCompany>, PlanCompanyRepository>();
      services.AddTransient<IPlanCompanyRepository, PlanCompanyRepository>();
      services.AddTransient<IBaseService<PlanCompany>, PlanCompanyService>();
      //ServiceProvided
      services.AddTransient<IServiceProvidedService, ServiceProvidedService>();
      services.AddTransient<IGenericRepository<ServiceProvided>, ServiceProvidedRepository>();
      services.AddTransient<IServiceProvidedRepository, ServiceProvidedRepository>();
      services.AddTransient<IBaseService<ServiceProvided>, ServiceProvidedService>();
      //Budget
      services.AddTransient<IBudgetService, BudgetService>();
      services.AddTransient<IGenericRepository<Budget>, BudgetRepository>();
      services.AddTransient<IBudgetRepository, BudgetRepository>();
      services.AddTransient<IBaseService<Budget>, BudgetService>();
      //BudgetItems
      services.AddTransient<IBudgetItemsService, BudgetItemsService>();
      services.AddTransient<IGenericRepository<BudgetItems>, BudgetItemsRepository>();
      services.AddTransient<IBudgetItemsRepository, BudgetItemsRepository>();
      services.AddTransient<IBaseService<BudgetItems>, BudgetItemsService>();
      //ServicesProvision
      services.AddTransient<IServicesProvisionService, ServicesProvisionService>();
      services.AddTransient<IGenericRepository<ServicesProvision>, ServicesProvisionRepository>();
      services.AddTransient<IServicesProvisionRepository, ServicesProvisionRepository>();
      services.AddTransient<IBaseService<ServicesProvision>, ServicesProvisionService>();
      //ServicesProvisionItems
      services.AddTransient<IServicesProvisionItemsService, ServicesProvisionItemsService>();
      services.AddTransient<IGenericRepository<ServicesProvisionItems>, ServicesProvisionItemsRepository>();
      services.AddTransient<IServicesProvisionItemsRepository, ServicesProvisionItemsRepository>();
      services.AddTransient<IBaseService<ServicesProvisionItems>, ServicesProvisionItemsService>();

      //BudgetPerformed
      services.AddTransient<IBudgetPerformedService, BudgetPerformedService>();
      services.AddTransient<IGenericRepository<BudgetPerformed>, BudgetPerformedRepository>();
      services.AddTransient<IBudgetPerformedRepository, BudgetPerformedRepository>();
      services.AddTransient<IBaseService<BudgetPerformed>, BudgetPerformedService>();
      #endregion
      //Salesman
      services.AddTransient<ISalesmanService, SalesmanService>();
      services.AddTransient<IGenericRepository<Salesman>, SalesmanRepository>();
      services.AddTransient<ISalesmanRepository, SalesmanRepository>();
      services.AddTransient<IBaseService<Salesman>, SalesmanService>();
      //Sale
      services.AddTransient<ISaleService, SaleService>();
      services.AddTransient<IGenericRepository<Sale>, SaleRepository>();
      services.AddTransient<ISaleRepository, SaleRepository>();
      services.AddTransient<IBaseService<Sale>, SaleService>();
      //SaleItems
      services.AddTransient<ISaleItemsService, SaleItemsService>();
      services.AddTransient<IGenericRepository<SaleItems>, SaleItemsRepository>();
      services.AddTransient<ISaleItemsRepository, SaleItemsRepository>();
      services.AddTransient<IBaseService<SaleItems>, SaleItemsService>();
      //Commission
      services.AddTransient<ICommissionService, CommissionService>();
      services.AddTransient<IGenericRepository<Commission>, CommissionRepository>();
      services.AddTransient<ICommissionRepository, CommissionRepository>();
      services.AddTransient<IBaseService<Commission>, CommissionService>();
      //CostCenter
      services.AddTransient<ICostCenterService, CostCenterService>();
      services.AddTransient<IGenericRepository<CostCenter>, CostCenterRepository>();
      services.AddTransient<ICostCenterRepository, CostCenterRepository>();
      services.AddTransient<IBaseService<CostCenter>, CostCenterService>();
      //Financial
      services.AddTransient<IFinancialService, FinancialService>();
      services.AddTransient<IGenericRepository<Financial>, FinancialRepository>();
      services.AddTransient<IFinancialRepository, FinancialRepository>();
      services.AddTransient<IBaseService<Financial>, FinancialService>();
      //Prospects
      services.AddTransient<IProspectsService, ProspectsService>();
      services.AddTransient<IGenericRepository<Prospects>, ProspectsRepository>();
      services.AddTransient<IProspectsRepository, ProspectsRepository>();
      services.AddTransient<IBaseService<Prospects>, ProspectsService>();
     // PhasesProspects
      services.AddTransient<IPhasesProspectsService, PhasesProspectsService>();
      services.AddTransient<IGenericRepository<PhasesProspects>, PhasesProspectsRepository>();
      services.AddTransient<IPhasesProspectsRepository, PhasesProspectsRepository>();
      services.AddTransient<IBaseService<PhasesProspects>, PhasesProspectsService>();
      //Product
      services.AddTransient<IProductService, ProductService>();
      services.AddTransient<IGenericRepository<Product>, ProductRepository>();
      services.AddTransient<IProductRepository, ProductRepository>();
      services.AddTransient<IBaseService<Product>, ProductService>();
      //SharedCommission
      services.AddTransient<ISharedCommissionService, SharedCommissionService>();
      services.AddTransient<IGenericRepository<SharedCommission>, SharedCommissionRepository>();
      services.AddTransient<ISharedCommissionRepository, SharedCommissionRepository>();
      services.AddTransient<IBaseService<SharedCommission>, SharedCommissionService>();
      //Closures
      services.AddTransient<IClosuresService, ClosuresService>();
      services.AddTransient<IGenericRepository<Closures>, ClosuresRepository>();
      services.AddTransient<IClosuresRepository, ClosuresRepository>();
      services.AddTransient<IBaseService<Closures>, ClosuresService>();
      //ClosureDetail
      services.AddTransient<IClosuresDetailService, ClosuresDetailService>();
      services.AddTransient<IGenericRepository<ClosuresDetail>, ClosuresDetailRepository>();
      services.AddTransient<IClosuresDetailRepository, ClosuresDetailRepository>();
      services.AddTransient<IBaseService<ClosuresDetail>, ClosuresDetailService>();

      services.AddCors(options =>
      {
        options.AddPolicy("EnableCORS", builder =>
        {
          builder.AllowAnyOrigin().AllowAnyHeader().WithOrigins(
            new[] {"http://localhost:3000", "http://localhost:3001",
						"http://appservicebox.link","https://appservicebox.link",
						"https://tractuscommissions.com.br"}
            ). AllowAnyMethod().Build();
        });
      });

      services.AddCors();
      services.AddControllers();

      var key = Encoding.ASCII.GetBytes(Settings.Secret);
      services.AddAuthentication(x =>
      {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(x =>
      {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false
        };
      }
      );
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen(c => { //<-- NOTE 'Add' instead of 'Configure'
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Version = "v1",
          Title = "ToDo API",
          Description = "An ASP.NET Core Web API for managing ToDo items",
          TermsOfService = new Uri("https://example.com/terms"),
          Contact = new OpenApiContact
          {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
          },
          License = new OpenApiLicense
          {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
          }
        });
      });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      var cultureInfo = new CultureInfo("pt-BR");
      //cultureInfo.NumberFormat.CurrencySymbol = "€";

      CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
      CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
   
      }
      app.UseSwagger(options =>
      {
        options.SerializeAsV2 = true;
      });
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlanCompanyAPI v2"));

      app.UseMiddleware<ExceptionMiddleware>();

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();
      app.UseCors("EnableCORS");

      // Enable compression
      app.UseResponseCompression();
      app.UseStaticFiles();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
      UpdateDatabase(app);


    }

    private static void UpdateDatabase(IApplicationBuilder app)
    {
      using (var serviceScope = app.ApplicationServices
          .GetRequiredService<IServiceScopeFactory>()
          .CreateScope())
      {
        using (var context = serviceScope.ServiceProvider.GetService<ContextBase>())
        {
          context.Database.Migrate();
        }
      }
    }

  }
}
