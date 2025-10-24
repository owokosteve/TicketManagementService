using System;
using Microsoft.AspNetCore.Http;
using TicketManagerService.Models;

namespace TicketManagerService.DTOs;

public class UpdateTicketDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public TicketStatus? TicketStatus { get; set; }
    public DateTime PromiseDate { get; set; } 
    public List<IFormFile>? Attachments { get; set; }
}
