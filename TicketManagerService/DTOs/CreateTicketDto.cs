using System;
using TicketManagerService.Models;
using Microsoft.AspNetCore.Http;


namespace TicketManagerService.DTOs;

public class CreateTicketDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Assignee { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public DateTime PromiseDate { get; set; }
    public string? TimeZoneId { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}
