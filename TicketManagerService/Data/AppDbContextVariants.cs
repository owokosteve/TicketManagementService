using System;
using Microsoft.EntityFrameworkCore;

namespace TicketManagerService.Data;

public static class AppDbContextVariants
{
    public class PostgresTicketManagerDbContext : TicketManagerDbContext
    {
        public PostgresTicketManagerDbContext(DbContextOptions<TicketManagerDbContext> options) : base(options) { }
    }

    public class SqlServerTicketManagerDbContext : TicketManagerDbContext
    {
        public SqlServerTicketManagerDbContext(DbContextOptions<TicketManagerDbContext> options) : base(options) { }
    }

    public class MySQLTicketManagerDbContext : TicketManagerDbContext
    {
        public MySQLTicketManagerDbContext(DbContextOptions<TicketManagerDbContext> options) : base(options) { }
    }
}
