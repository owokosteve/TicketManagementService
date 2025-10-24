using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TicketManagerService.Models;

public class FileAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [ForeignKey("Ticket")]
    public int TicketId { get; set; }
    public required string AttachmentName { get; set; }
    public required string AttachmentType { get; set; }
    public long AttachmentSize { get; set; }
    public required string AttachmentUrl { get; set; }

    [JsonIgnore]
    public Ticket? Ticket { get; set; }
}
