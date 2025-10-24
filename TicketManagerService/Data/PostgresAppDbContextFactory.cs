using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static TicketManagerService.Data.AppDbContextVariants;

namespace TicketManagerService.Data;

public class PostgresAppDbContextFactory : IDesignTimeDbContextFactory<PostgresTicketManagerDbContext>
{
    public PostgresTicketManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TicketManagerDbContext>();
        var connectionString = "Host=localhost;Port=5432;Database=ticket_manager_pg;Username=postgres;Password=jagoro";
        optionsBuilder.UseNpgsql(connectionString);
        return new PostgresTicketManagerDbContext(optionsBuilder.Options);
    }
}
