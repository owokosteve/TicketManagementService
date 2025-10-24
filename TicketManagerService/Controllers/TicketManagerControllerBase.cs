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
    public async Task<ActionResult<List<Ticket>>> Tickets()
    {
        var tickets = await _ticketManagerHandler.GetTicketsAsync();
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

}
