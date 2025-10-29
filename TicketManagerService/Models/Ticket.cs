using System;
using System.ComponentModel.DataAnnotations;

namespace TicketManagerService.Models;

public class Ticket
{
    [Key]
    public int TicketId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Assignee { get; set; }
    public TicketPriority TicketPriority { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public DateTime PromiseDate { get; set; } = DateTime.UtcNow;
    public ICollection<TicketAttachment> TicketAttachments { get; set; } = [];
}

public enum TicketStatus { Open, InProgress, Resolved, Closed }
public enum TicketPriority { Critical, High, Medium, Low }
