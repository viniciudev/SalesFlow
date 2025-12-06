// DesignTimeContextFactory.cs (na raiz, mesmo nível do .csproj)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;
using System;

public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ContextBase>
{
    public ContextBase CreateDbContext(string[] args)
    {
        // Connection string FIXA - sem complicação
        var connectionString = "Host=localhost;Port=5432;Database=salesflowdb;Username=postgres;password=admin";

        var options = new DbContextOptionsBuilder<ContextBase>()
            .UseNpgsql(connectionString,
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds))
            .Options;

        return new ContextBase(options);
    }
}