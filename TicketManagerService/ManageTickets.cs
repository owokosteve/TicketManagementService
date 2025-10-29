using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NSCatch.Interfaces;
using NSCore.DatabaseContext;
using NSCore.DatabaseProviders;
using TicketManagerService.Data;
using TicketManagerService.DbImplementation;
using TicketManagerService.DTOs;
using TicketManagerService.Interfaces;
using TicketManagerService.Models;

namespace TicketManagerService;

/// <summary>
/// Manages ticket-related operations such as adding, deleting, and updating tickets.
/// </summary>
public class ManageTickets : BackgroundService, IManageTickets
{
    private IManageTickets _connection;
    private bool _isContextCreated;
    private bool _applyMigrationsAutomatically;
    private INsContextInit _initializer = null!;

    private readonly ICacheManager _cacheManager;
    private readonly ICacheKeyBuilder _keyBuilder;

    /// <summary>
    /// Initializes a new instance of the ManageUsers class.
    /// </summary>
    /// <param name="config">The database configuration.</param>
    /// <param name="contextFactory">The database context factory.</param>
    /// <param name="cacheManager">The cache manager instance.</param>
    /// <param name="keyBuilder">The cache key builder instance.</param>
    /// <param name="applyMigrationsAutomatically">Whether to apply migrations automatically.</param>
    public ManageTickets(IDatabaseConfig config, IDbContextFactory<TicketManagerDbContext> contextFactory,
        ICacheManager cacheManager, ICacheKeyBuilder keyBuilder, bool applyMigrationsAutomatically = true)
    {
        _applyMigrationsAutomatically = applyMigrationsAutomatically;
        _cacheManager = cacheManager;
        _keyBuilder = keyBuilder;

        _connection = config switch
        {
            SQLDb => new SQL(contextFactory),
            PSQLDb => new PostgreSQL(contextFactory),
            MySQLDb => new MySQL(contextFactory),
            _ => throw new ArgumentException("Unsupported database type.")
        };

        if (_connection == null) throw new InvalidOperationException("Failed to initialize a valid INsContextInitializer.");
        _initializer = _connection as INsContextInit ?? throw new InvalidOperationException("Failed to initialize a valid INsContextInitializer.");
    }

    #region Cache Key generators
    private string BuildTicketKey(int id)
    {
        using var sha256 = System.Security.Cryptography.SHA3_256.Create();
        var idBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(id.ToString()));
        var hashString = Convert.ToBase64String(idBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        string cacheKey = _keyBuilder.BuildKey(prefix: $"Ticket_{hashString}");
        return cacheKey;
    }

    private string BuildTicketsKey()
    {
        string cacheKey = _keyBuilder.BuildKey(prefix: $"Tickets_General");
        return cacheKey;
    }

    #endregion

    public async Task<Ticket> CreateTicketAsync(CreateTicketDto ticket)
    {
        EnsureContext();
        Ticket createdTicket;
        createdTicket = await _connection.CreateTicketAsync(ticket);

        var generalCache = BuildTicketsKey();
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCache);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Concat(new[] { createdTicket }).ToList();
            await _cacheManager.SetAsync(generalCache, updatedTickets);
        }
        
        return createdTicket;
    }

    public async Task<Ticket> DeleteTicketAsync(int ticketId)
    {
        EnsureContext();

        var ticketCacheKey = BuildTicketKey(ticketId);
        await _cacheManager.RemoveAsync(ticketCacheKey);

        var generalCacheKey = BuildTicketsKey();
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Where(t => t.TicketId != ticketId).ToList();
            await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        }
        else
        {
            await _cacheManager.RemoveAsync(generalCacheKey);
        }

        var ticket = await _connection.DeleteTicketAsync(ticketId);
        return ticket;
    }

    public async Task<TicketAttachment?> GetTicketAttachmentAsync(int attachmentId)
    {
        EnsureContext();
        var attachment = await _connection.GetTicketAttachmentAsync(attachmentId);
        return attachment;
    }

    public async Task DownloadAttachment(int ticketId, int attachementId)
    {
        EnsureContext();
        await _connection.DownloadAttachment(ticketId, attachementId);
    }

    public async Task<int> GetNumberOfTicketsAsync()
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsAsync();
    }

    public async Task<int> GetFilteredNumberOfTicketsAsync(Func<Ticket, bool> predicate)
    {
        EnsureContext();
        return await _connection.GetFilteredNumberOfTicketsAsync(predicate);
    }

    public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<Ticket?>(BuildTicketKey(ticketId), () => _connection.GetTicketByIdAsync(ticketId));
    }

    public async Task<List<Ticket>> GetTicketsAsync()
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<List<Ticket>>(key: BuildTicketsKey(), () => _connection.GetTicketsAsync());
    }

    public async Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDto ticket)
    {
        EnsureContext();
        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketAsync(ticketId, ticket);

        var ticketCacheKey = BuildTicketKey(ticketId);
        await _cacheManager.RemoveAsync(ticketCacheKey);

        var generalCacheKey = BuildTicketsKey();
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Where(t => t.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
            await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        }

        return updatedTicket;
    }

    public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
    {
        EnsureContext();
        return await _connection.UpdateTicketStatusAsync(ticketId, status);
    }

    public async Task RemoveTicketAttachmentAsync(int attachmentId)
    {
        EnsureContext();
        await _connection.RemoveTicketAttachmentAsync(attachmentId);
    }


    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("Database connection is not initialized.");
        }

        if (_applyMigrationsAutomatically)
        {
            await _initializer.ApplyMigrationsAsync(stoppingToken);
        }

        if (_initializer.IsContextCreated())
        {
            _isContextCreated = true;
        }
    }

    /// <summary>
    /// Ensures that the context has been properly initialized.
    /// Throws a NotInitializedException if the context is not created.
    /// </summary>
    private void EnsureContext()
    {
        if (!_isContextCreated)
        {
            throw new InvalidOperationException("Failed to initialize the database context.");
        }
    }

    public Task<Ticket?> GetTicketByIdAsync(int ticketId, bool includeAttachments = true)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetTicketsAsync(bool includeAttachments = true)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetFilteredTicketsAsync(Func<Ticket, bool> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetTicketByDateRange(DateOnly startDate, DateOnly endDate)
    {
        throw new NotImplementedException();
    }
}
