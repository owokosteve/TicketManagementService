using System;
using Microsoft.EntityFrameworkCore;
using TicketManagerService.Models;

namespace TicketManagerService.Data;

public class TicketManagerDbContext : DbContext
{
    public TicketManagerDbContext(DbContextOptions<TicketManagerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder.Entity<Ticket>(ticketEntity =>
        {
            ticketEntity.Property(t => t.TicketStatus).HasConversion<string>();
        }));
    }

    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketAttachment> TicketAttachments { get; set; } = null!;
}
