using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using TicketManagerService.Data;
using static TicketManagerService.Data.AppDbContextVariants;

namespace TicketManagerService.DbImplementation;

public class MySQL : BaseDbImplementation
{
    public MySQL(IDbContextFactory<TicketManagerDbContext> contextFactory) : base(contextFactory) { }

    protected override TicketManagerDbContext CreateMigrationContext()
    {
        using var baseContext = _contextFactory.CreateDbContext();
        var connectionString = baseContext.Database.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<TicketManagerDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new MySQLTicketManagerDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the SQL statement for marking a migration as applied in MySQL.
    /// </summary>
    /// <returns>The MySQL-specific SQL statement.</returns>
    protected override string GetMarkMigrationSql()
    {
        return @"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES (@migrationId, @productVersion) ON CONFLICT (""MigrationId"") DO NOTHING;";
    }

    /// <summary>
    /// Gets the parameters for marking a migration as applied in MySQL.
    /// </summary>
    /// <param name="migrationId">The migration ID.</param>
    /// <param name="productVersion">The product version.</param>
    /// <returns>The MySQL-specific parameters array.</returns>
    protected override object[] GetMarkMigrationParameters(string migrationId, string productVersion)
    {
        return new[]
        {
            new MySqlConnector.MySqlParameter("@migrationId", migrationId),
            new MySqlConnector.MySqlParameter("@productVersion", productVersion)
        };
    }

    /// <summary>
    /// Determines if the exception indicates a table already exists error for MySQL.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception indicates a table already exists error in MySQL.</returns>
    protected override bool IsTableAlreadyExistsError(Exception ex)
    {
        return ex is MySqlException sqlEx && sqlEx.Number == 2714;
    }
}
