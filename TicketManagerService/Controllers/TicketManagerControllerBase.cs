using System;
using Microsoft.AspNetCore.Mvc;
using TicketManagerService.DTOs;
using TicketManagerService.Interfaces;
using TicketManagerService.Models;

namespace TicketManagerService.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketManagerControllerBase : ControllerBase
{
    private readonly IManageTickets _ticketManagerHandler;

    public TicketManagerControllerBase(IManageTickets ticketManagerHandler)
    {
        _ticketManagerHandler = ticketManagerHandler ?? throw new ArgumentNullException(nameof(ticketManagerHandler));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Ticket([FromForm] CreateTicketDto createTicket)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var ticket = await _ticketManagerHandler.CreateTicketAsync(createTicket);
            return Created("", new { Message = "Ticket Created Successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Ticket(int id)
    {
        var ticket = await _ticketManagerHandler.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound($"Ticket with ID {id} not found.");
        return Ok(ticket);
    }

    [HttpGet]
    public async Task<ActionResult<List<Ticket>>> Tickets(bool includeAttachments)
    {
        var tickets = await _ticketManagerHandler.GetTicketsAsync(includeAttachments);
        return Ok(tickets);
    }

    /// <summary>
    /// Updates an existing ticket's information.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket to update.</param>
    /// <param name="updateTicket">Ticket update data transfer object.</param>
    /// <returns>No content on success.</returns>
    /// <response code='204'>Ticket updated successfully</response>
    /// <response code='400'>Invalid ticket data.</response>
    /// <response code='404'>Ticket not found.</response>
    [HttpPut("{ticketId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int ticketId, [FromForm] UpdateTicketDto updateTicket)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _ticketManagerHandler.UpdateTicketAsync(ticketId, updateTicket);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{ticketId:int}")]
    public async Task<IActionResult> Delete(int ticketId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _ticketManagerHandler.DeleteTicketAsync(ticketId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{ticketId}/attachment/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(int ticketId, int attachmentId)
    {
        var attachment = await _ticketManagerHandler.GetTicketAttachmentAsync(attachmentId);
        if (attachment == null) return NotFound();
        if (attachment.TicketId != ticketId) return NotFound();
        if (!System.IO.File.Exists(attachment.AttachmentUrl)) return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.AttachmentUrl);
        var contentType = "application/octet-stream";
        return File(fileBytes, contentType);
    }

    [HttpGet("ticketsByStatus")]
    public async Task<IActionResult> TicketsByStatus(TicketStatus status, bool includeAttachments)
    {
        var results = await _ticketManagerHandler.GetFilteredTicketsAsync(t => t.TicketStatus == status, includeAttachments=false);
        return Ok(results);
    }

    [HttpGet("ticketByPriority")]
    public async Task<IActionResult> TicketsByPriority(TicketPriority priority)
    {
        var results = await _ticketManagerHandler.GetFilteredTicketsAsync(t => t.TicketPriority == priority);
        return Ok(results);
    }

    [HttpGet("ticketByAssignee")]
    public async Task<IActionResult> TicketsByAssignee(string assignee)
    {
        var results = await _ticketManagerHandler.GetFilteredTicketsAsync(t => t.Assignee == assignee);
        return Ok(results);
    }

    // Get total number of tickets
    [HttpGet("ticketsCount")]
    public async Task<IActionResult> NumberOfTickets()
    {
        var numberOfTickets = await _ticketManagerHandler.GetNumberOfTicketsAsync();
        return Ok(numberOfTickets);
    }

    // Get total number of tickets by status
    [HttpGet("ticketsCountByStatus")]
    public async Task<IActionResult> NumberOfTicketsByStatus(TicketStatus status)
    {
        var results = await _ticketManagerHandler.GetFilteredNumberOfTicketsAsync(ticket => ticket.TicketStatus == status);
        return Ok(results);
    }

    // Get total number of tickets by Priority
    [HttpGet("ticketsCountByPriority")]
    public async Task<IActionResult> NumberOfTicketsByPriority(TicketPriority priority)
    {
        var results = await _ticketManagerHandler.GetFilteredNumberOfTicketsAsync(ticket => ticket.TicketPriority == priority);
        return Ok(results);
    }

    // Get total number of tickets by assignee
    [HttpGet("ticketsCountByAssignee")]
    public async Task<IActionResult> NumberOfTicketsByAssignee(string assignee)
    {
        var results = await _ticketManagerHandler.GetFilteredNumberOfTicketsAsync(ticket => ticket.Assignee == assignee);
        return Ok(results);
    }

}
