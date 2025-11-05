using System;
using System.Linq.Expressions;
using TicketManagerService.DTOs;
using TicketManagerService.Models;

namespace TicketManagerService.Interfaces;

/// <summary>
/// Provides methods for managing tickets in the system.
/// </summary>
public interface IManageTickets
{
    /// <summary>
    /// Adds a new ticket to the system using CreateTicketDto object.
    /// </summary>
    /// <param name="ticket">The CreateTicketDto object containing ticket details.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<Ticket> CreateTicketAsync(CreateTicketDto ticket);

    /// <summary>
    /// Delete a ticket by their IDs.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to delete.</param>
    /// <returns>A task that represents the asynchronous operations.</returns>
    Task<Ticket> DeleteTicketAsync(int ticketId);

    /// <summary>
    /// Retrieves a ticket by ticket ID.
    /// </summary>
    /// <param name="ticketId">The TicketId of the ticket to retrieve.</param>
    /// <param name="IncludeAttachments">Whether to include attachment in the result. Defaults to true.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the ticket, or null if not found.
    /// </returns>
    Task<Ticket?> GetTicketByIdAsync(int ticketId, bool includeAttachments = true);

    /// <summary>
    /// Retrieves all tickets
    /// </summary>
    /// <param name="IncludeAttachments">Whether to include attachment in the result. Defaults to true.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result containes the a list of tickets, or null if not found.
    /// </returns>
    Task<List<Ticket>> GetTicketsAsync(bool includeAttachments = true);

    /// <summary>
    /// Retrieves tickets according to the specified preidcate
    /// </summary>
    /// <param name="predicate">The predicate name to filter</param>
    /// <returns>
    /// A task represents the asynchronous operation. The task result contains the list filtered tickets according to specified predicate.
    /// </returns>
    Task<List<Ticket>> GetFilteredTicketsAsync(Func<Ticket, bool> predicate, bool includeAttachments = true);

    /// <summary>
    /// Updates the details of a ticket
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update.</param>
    /// <param name="ticket">The updated ticket details.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDto ticket);
    
    /// <summary>
    /// Update the status of the ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update.</param>
    /// <param name="status">The new status of the ticket.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status);

    /// <summary>
    /// Retrieves the ticket attachment of given ticket.
    /// </summary>
    /// <param name="attachmentId">The ID of the ticket attachment.</param>
    /// <returns>A task that represent the asynchrouns operations.</returns>
    Task<TicketAttachment?> GetTicketAttachmentAsync(int attachmentId);

    /// <summary>
    /// Updates ticket attachment only
    /// </summary>
    /// <param name="ticketId">The Ticket ID of the attachment</param>
    /// <param name="updateAttachmentRequestDto">Ticket attachment request Dto.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    Task UploadTicketAttachmentAsync(int ticketId, bool removePrevious, UpdateAttachmentRequestDto updateAttachmentRequestDto);

    /// <summary>
    /// Deletes a ticket attachment.
    /// </summary>
    /// <param name="attachment">The Ticket attachment.</param>
    /// <returns>A task that represents the asynchrounous operation.</returns>
    Task RemoveTicketAttachmentAsync(TicketAttachment attachment);

    /// <summary>
    /// Gets the total number of tickets.
    /// </summary>A task that represents the asynchronous operation.
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<int> GetNumberOfTicketsAsync();

    /// <summary>
    /// Retrieves number of tickets by specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate name to filter.</param>
    /// <returns>A task that represent the asynchronous operation.</returns>
    Task<int> GetFilteredNumberOfTicketsAsync(Expression<Func<Ticket, bool>> predicate);

    /// <summary>
    /// Retrierves a ticket based on date range.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<List<Ticket>> GetTicketByDateRangeAsync(DateTime startDate, DateTime endDate);
}
