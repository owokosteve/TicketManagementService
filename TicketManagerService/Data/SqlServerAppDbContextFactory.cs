using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static TicketManagerService.Data.AppDbContextVariants;

namespace TicketManagerService.Data;

public class SqlServerAppDbContextFactory : IDesignTimeDbContextFactory<SqlServerTicketManagerDbContext>
{
    public SqlServerTicketManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TicketManagerDbContext>();
        // var connectionString = "Server=localhost;Database=file_manager_sql;User Id=sa;Password=yourpassword;";
        var connectionString = "Server=localhost;Database=ticket_manager_sql;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString);
        return new SqlServerTicketManagerDbContext(optionsBuilder.Options);
    }
}

