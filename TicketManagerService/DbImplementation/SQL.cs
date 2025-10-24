using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TicketManagerService.Data;
using static TicketManagerService.Data.AppDbContextVariants;

namespace TicketManagerService.DbImplementation;

public class SQL : BaseDbImplementation
{
    public SQL(IDbContextFactory<TicketManagerDbContext> contextFactory) : base(contextFactory) { }

    protected override TicketManagerDbContext CreateMigrationContext()
    {
        using var baseContext = _contextFactory.CreateDbContext();
        var connectionString = baseContext.Database.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<TicketManagerDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SqlServerTicketManagerDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the SQL statement for marking a migration as applied in SQLServer.
    /// </summary>
    /// <returns>The SQLServer-specific SQL statement.</returns>
    protected override string GetMarkMigrationSql()
    {
        return @"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES (@migrationId, @productVersion) ON CONFLICT (""MigrationId"") DO NOTHING;";
    }

    /// <summary>
    /// Gets the parameters for marking a migration as applied in SQLServer.
    /// </summary>
    /// <param name="migrationId">The migration ID.</param>
    /// <param name="productVersion">The product version.</param>
    /// <returns>The SqlServer-specific parameters array.</returns>
    protected override object[] GetMarkMigrationParameters(string migrationId, string productVersion)
    {
        return new[]
        {
            new Microsoft.Data.SqlClient.SqlParameter("@migrationId", migrationId),
            new Microsoft.Data.SqlClient.SqlParameter("@productVersion", productVersion)
        };
    }

    /// <summary>
    /// Determines if the exception indicates a table already exists error for SQLServer.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception indicates a table already exists error in SQLServer.</returns>
    protected override bool IsTableAlreadyExistsError(Exception ex)
    {
        return ex is SqlException sqlEx && sqlEx.Number == 2714;
    }
}
