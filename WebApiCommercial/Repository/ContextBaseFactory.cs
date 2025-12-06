// Data/ContextBaseFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;
using System;

namespace Model
{
    public class ContextBaseFactory : IDesignTimeDbContextFactory<ContextBase>
    {
        public ContextBase CreateDbContext(string[] args)
        {
            // 1. Tenta pegar de variável de ambiente
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            // 2. Se não tiver, usa valor padrão
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Port=5432;Database=salesflowdb;Username=postgres";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ContextBase>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ContextBase(optionsBuilder.Options);
        }
    }
}