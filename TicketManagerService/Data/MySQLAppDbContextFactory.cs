using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static TicketManagerService.Data.AppDbContextVariants;

namespace TicketManagerService.Data;

public class MySQLAppDbContextFactory : IDesignTimeDbContextFactory<MySQLTicketManagerDbContext>
{
    public MySQLTicketManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TicketManagerDbContext>();
        // TO-DO: Update this connection string as needed
        var connectionString = "Server=localhost;Database=ticket_manager_mysql;User=root;Password=35688410;";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new MySQLTicketManagerDbContext(optionsBuilder.Options);
    }
}
