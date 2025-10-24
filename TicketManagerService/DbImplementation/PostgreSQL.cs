using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TicketManagerService.Data;
using static TicketManagerService.Data.AppDbContextVariants;

namespace TicketManagerService.DbImplementation;

public class PostgreSQL : BaseDbImplementation
{
    public PostgreSQL(IDbContextFactory<TicketManagerDbContext> contextFactory) : base(contextFactory) { }

    protected override TicketManagerDbContext CreateMigrationContext()
    {
        using var baseContext = _contextFactory.CreateDbContext();
        var connectionString = baseContext.Database.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<TicketManagerDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        return new PostgresTicketManagerDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the SQL statement for marking a migration as applied in PostgreSQL.
    /// </summary>
    /// <returns>The PostgreSQL-specific SQL statement.</returns>
    protected override string GetMarkMigrationSql()
    {
        return @"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES (@migrationId, @productVersion) ON CONFLICT (""MigrationId"") DO NOTHING;";
    }

    /// <summary>
    /// Gets the parameters for marking a migration as applied in PostgreSQL.
    /// </summary>
    /// <param name="migrationId">The migration ID.</param>
    /// <param name="productVersion">The product version.</param>
    /// <returns>The PostgreSQL-specific parameters array.</returns>
    protected override object[] GetMarkMigrationParameters(string migrationId, string productVersion)
    {
        return new[]
        {
            new NpgsqlParameter("@migrationId", migrationId),
            new NpgsqlParameter("@productVersion", productVersion)
        };
    }

    /// <summary>
    /// Determines if the exception indicates a table already exists error for PostgreSQL.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception indicates a table already exists error in PostgreSQL.</returns>
    protected override bool IsTableAlreadyExistsError(Exception ex)
    {
        return ex is PostgresException pgEx && pgEx.SqlState == "42P07";
    }
}
