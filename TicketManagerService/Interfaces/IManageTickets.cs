using System;
using TicketManagerService.DTOs;
using TicketManagerService.Models;

namespace TicketManagerService.Interfaces;

public interface IManageTickets
{
    Task<Ticket> CreateTicketAsync(CreateTicketDto ticket);
    Task<Ticket> DeleteTicketAsync(int ticketId);
    Task<Ticket> GetTicketByIdAsync(int ticketId);
    Task<List<Ticket>> GetTicketsAsync();
    Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDto ticket);
    Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status);
    Task<FileAttachment> GetTicketAttachmentAsync(int attachmentId);
    Task DownloadAttachment(int ticketId, int attachementId);
    Task RemoveTicketAttachmentAsync(int attachmentId);
    Task<int> GetNumberOfTicketsAsync();
    Task<int> GetNumberOfTicketsByStatusAsync(TicketStatus status);
}
