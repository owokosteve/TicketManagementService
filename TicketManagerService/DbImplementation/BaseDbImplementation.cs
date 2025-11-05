using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSCore.DatabaseContext;
using TicketManagerService.Data;
using TicketManagerService.DTOs;
using TicketManagerService.Interfaces;
using TicketManagerService.Models;

namespace TicketManagerService.DbImplementation;

/// <summary>
/// Base implementation class for user management operations.
/// Contains common logic shared across all database providers.
/// </summary>
public abstract class BaseDbImplementation : INsContextInit, IManageTickets, IDisposable
{
    protected bool _disposed = false;
    protected readonly IDbContextFactory<TicketManagerDbContext> _contextFactory = null!;

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the BaseDbImplementation class.
    /// </summary>
    /// <param name="contextFactory">The database context factory.</param>
    protected BaseDbImplementation(IDbContextFactory<TicketManagerDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        InitializeConnection();
    }

    /// <summary>
    /// Initializes the database connection. Can be overridden by derived classes.
    /// </summary>
    protected virtual void InitializeConnection()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            if (!context.Database.CanConnect())
            {
                throw new InvalidOperationException("Unable to connect to the database. Please check the connection string and database server.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize the database context.", ex);
        }
    }
    #endregion

    #region Migration Methods
    /// <inheritdoc/>
    /// <summary>
    /// Creates a database context for migration operations.
    /// Can be overridden by derived classes to provide database-specific contexts.
    /// </summary>
    /// <returns>The database context to use for migrations.</returns>
    protected virtual TicketManagerDbContext CreateMigrationContext()
    {
        return _contextFactory.CreateDbContext();
    }

    /// <summary>
    /// Applies migrations to the database asynchronously.
    /// This method uses the database-specific context for proper migration discovery.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = CreateMigrationContext();
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Database doesn't exist. Please create and configure it first.");
            }

            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
        }
        catch (Exception ex) when (IsTableAlreadyExistsError(ex))
        {
            await HandleMigrationConflictAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Migration failed: {ex.Message}", ex);
        }
    }

    protected virtual bool IsTableAlreadyExistsError(Exception ex)
    {
        // Base implementation - override in derived classes for specific database error codes
        return false;
    }

    /// <summary>
    /// Marks a migration as applied in the database.
    /// Can be overridden by derived classes for database-specific implementation.
    /// </summary>
    /// <param name="migrationId">The migration ID to mark as applied.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected virtual async Task MarkMigrationAsAppliedAsync(string migrationId, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();
        var provider = context.Database.ProviderName;
        var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "8.0.0";

        // This is a generic implementation - override in derived classes for database-specific SQL
        var rawQuery = GetMarkMigrationSql();
        var parameters = GetMarkMigrationParameters(migrationId, productVersion);

        await context.Database.ExecuteSqlRawAsync(rawQuery, parameters, cancellationToken);
    }

    /// <summary>
    /// Gets the SQL statement for marking a migration as applied.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <returns>The SQL statement.</returns>
    protected abstract string GetMarkMigrationSql();

    /// <summary>
    /// Gets the parameters for marking a migration as applied.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <param name="migrationId">The migration ID.</param>
    /// <param name="productVersion">The product version.</param>
    /// <returns>The parameters array.</returns>
    protected abstract object[] GetMarkMigrationParameters(string migrationId, string productVersion);

    private async Task HandleMigrationConflictAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var allMigrations = context.Database.GetMigrations().ToList();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var latestMigration = allMigrations.LastOrDefault();

            if (latestMigration != null && pendingMigrations.Contains(latestMigration))
            {
                foreach (var migration in pendingMigrations)
                {
                    await MarkMigrationAsAppliedAsync(migration, cancellationToken);
                }
            }
            else
            {
                throw new InvalidOperationException("Could not resolve conflict: No valid latest migration found.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to resolve migration conflict: {ex.Message}", ex);
        }
    }
    #endregion

    /// <inheritdoc/>
    public virtual bool IsContextCreated()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Database.CanConnect();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check if the context is created.", ex);
        }
    }


    /// <inheritdoc/>
    public virtual async Task<Ticket> CreateTicketAsync(CreateTicketDto createTicketDto)
    {
        if (createTicketDto == null) throw new ArgumentNullException(nameof(createTicketDto));

        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        var ticket = new Ticket
        {
            Title = createTicketDto.Title,
            Description = createTicketDto.Description,
            Assignee = createTicketDto.Assignee,
            TicketPriority = createTicketDto.TicketPriority,
            TicketStatus = createTicketDto.TicketStatus,
            PromiseDate = DateTime.UtcNow
        };

        if (createTicketDto.TicketAttachments != null && createTicketDto.TicketAttachments.Any())
        {
            var uploadsPath = Path.Combine("uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Upload attachments
            foreach (var ticketAttachment in createTicketDto.TicketAttachments)
            {
                var filePath = Path.Combine(uploadsPath, ticketAttachment.FileName.Trim().ToLower());
                using (var fstream = new FileStream(filePath, FileMode.Create)) await ticketAttachment.CopyToAsync(fstream);
                var newTicketAttachment = new TicketAttachment
                {
                    AttachmentName = ticketAttachment.FileName,
                    AttachmentType = ticketAttachment.ContentType,
                    AttachmentSize = ticketAttachment.Length,
                    AttachmentUrl = filePath
                };
                ticket.TicketAttachments.Add(newTicketAttachment);
            }
        }

        try
        {
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ticket;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Failed to add ticket due to database error. {ex}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"An unexpected error occured while adding ticket. {ex}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Ticket> DeleteTicketAsync(int ticketId)
    {
        if (ticketId <= 0) throw new ArgumentException($"User ID must be greater than zero. {nameof(ticketId)}");

        try
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            var ticket = await context.Tickets.Include(a => a.TicketAttachments).FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (ticket == null) throw new InvalidOperationException($"Ticket with ID {ticketId} not found");

            // Remove from disk
            foreach (var attachment in ticket.TicketAttachments)
            {
                if (System.IO.File.Exists(attachment.AttachmentUrl))
                    System.IO.File.Delete(attachment.AttachmentUrl);
            }

            // Remove records from database.
            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ticket;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to delete ticket due to a database error. {ex}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An unexpected error occured while deleting ticket. {ex}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Ticket?> GetTicketByIdAsync(int ticketId, bool includeAttachments = false)
    {
        if (ticketId <= 0) throw new ArgumentException($"Ticket ID must be greater than zero. {nameof(ticketId)}");

        try
        {
            using var context = _contextFactory.CreateDbContext();
            if (includeAttachments)
                return await context.Tickets.Include(t => t.TicketAttachments).FirstOrDefaultAsync(t => t.TicketId == ticketId);
            else
                return await context.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving ticket. {ex}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<List<Ticket>> GetTicketsAsync(bool includeAttachments = true)
    {
        using var context = _contextFactory.CreateDbContext();

        try
        {
            if (includeAttachments)
                return await context.Tickets.Include(a => a.TicketAttachments).ToListAsync();
            else
                return await context.Tickets.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving tickets. {ex}");
        }

    }

    /// <inheritdoc/>
    public async Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDto updateTicketDto)
    {
        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        var ticket = await GetTicketByIdAsync(ticketId);

        if (!string.IsNullOrWhiteSpace(updateTicketDto.Title)) ticket!.Title = updateTicketDto.Title;
        if (!string.IsNullOrWhiteSpace(updateTicketDto.Description)) ticket!.Description = updateTicketDto.Description;
        ticket!.TicketPriority = updateTicketDto.TicketPriority ?? ticket.TicketPriority;
        ticket.TicketStatus = updateTicketDto.TicketStatus ?? ticket.TicketStatus;
        ticket.PromiseDate = DateTime.UtcNow;


        // Add attachments
        if (updateTicketDto.TicketAttachments != null && updateTicketDto.TicketAttachments.Any())
        {
            var uploadsPath = Path.Combine("uploads");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            foreach (var attachment in updateTicketDto.TicketAttachments)
            {
                var filePath = Path.Combine(uploadsPath, attachment.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create)) await attachment.CopyToAsync(stream);

                var newAttachment = new TicketAttachment
                {
                    AttachmentName = attachment.FileName,
                    AttachmentType = attachment.ContentType,
                    AttachmentUrl = filePath,
                    AttachmentSize = attachment.Length,
                };
                ticket.TicketAttachments.Add(newAttachment);
            }
        }

        try
        {
            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ticket;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Failed to update ticket due to database error: {ex}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"An unexpected error occured while updating ticket: {ex}");
        }

    }

    public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
    {
        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        var ticket = await GetTicketByIdAsync(ticketId);
        ticket!.TicketStatus = status;

        try
        {
            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ticket;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Failed to update ticket status due to database error: {ex}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"An unexpected error occured while updating ticket status: {ex}");
        }
    }

    public virtual async Task UploadTicketAttachmentAsync(int ticketId, bool removePrevious, UpdateAttachmentRequestDto updateAttachmentRequestDto)
    {
        if (ticketId <= 0) throw new ArgumentException($"Ticket ID must be greater than zero. {nameof(ticketId)}");
        if (updateAttachmentRequestDto == null) throw new ArgumentNullException(nameof(updateAttachmentRequestDto));

        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        // var ticketAttachment = new TicketAttachment
        // {
        //     AttachmentName = updateAttachmentRequestDto.AttachmentName,
        //     AttachmentType = updateAttachmentRequestDto.AttachmentType,
        //     AttachmentSize = updateAttachmentRequestDto.AttachmentSize,
        //     AttachmentUrl = updateAttachmentRequestDto.AttachmentUrl
        // };

        var ticket = await GetTicketByIdAsync(ticketId);

        if (ticket!.TicketStatus == TicketStatus.Open || ticket.TicketStatus == TicketStatus.InProgress)
        {
            if (updateAttachmentRequestDto != null && updateAttachmentRequestDto.NewTicketAttachment.Any())
            {
                // Remove the previous attachment if remove previous = True
                if (removePrevious == true)
                {
                    var attachmentsToDelete = ticket.TicketAttachments.ToList();

                    foreach (var attachment in attachmentsToDelete)
                    {
                        if (System.IO.File.Exists(attachment.AttachmentUrl))
                            System.IO.File.Delete(attachment.AttachmentUrl);

                        var fileAttachment = await GetTicketAttachmentAsync(attachment.AttachmentId);
                        try
                        {
                            await RemoveTicketAttachmentAsync(fileAttachment!);
                        }
                        catch (DbUpdateException ex)
                        {
                            await transaction.RollbackAsync();
                            throw new InvalidOperationException($"Failed to upload ticket attachment due to database error. {ex}");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw new InvalidOperationException($"An unexpected error occured while uploading attachment. {ex}");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("No ticket attachment provided.");
            }
        }
    }

    public virtual async Task<TicketAttachment?> GetTicketAttachmentAsync(int attachmentId)
    {
        if (attachmentId <= 0) throw new ArgumentException($"Attachment ID must be greater than zero. {nameof(attachmentId)}");

        using var context = _contextFactory.CreateDbContext();

        try
        {
            var attachment = await context.TicketAttachments.FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);
            return attachment;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving ticket attachment. {ex}");
        }
    }

    public async Task RemoveTicketAttachmentAsync(TicketAttachment attachment)
    {

        using var context = _contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            context.TicketAttachments.Remove(attachment!);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occured while deleting ticket attachment. {ex}");
        }
    }

    public async Task<int> GetNumberOfTicketsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Tickets.CountAsync();
    }

    public Task<int> GetNumberOfTicketsByStatusAsync(TicketStatus status)
    {
        using var context = _contextFactory.CreateDbContext();
        var ticketsByStatus = context.Tickets.Where(t => t.TicketStatus == status).CountAsync();
        return ticketsByStatus;
    }

    public async Task<List<Ticket>> GetFilteredTicketsAsync(Func<Ticket, bool> predicate, bool includeAttachments)
    {
        var tickets = await GetTicketsAsync(includeAttachments);
        return (List<Ticket>)tickets.Where(predicate).ToList();
    }

    public async Task<int> GetFilteredNumberOfTicketsAsync(Expression<Func<Ticket, bool>> predicate)
    {
        var context = _contextFactory.CreateDbContext();
        return context.Tickets.Where(predicate).Count();
    }

    public async Task<List<Ticket>> GetTicketByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var tickets = await GetTicketsAsync();
        return tickets.Where(t => t.PromiseDate >= startDate && t.PromiseDate <= endDate).ToList();
    }

    #region Dispose Pattern
    /// <summary>
    /// Disposes the resources used by the BaseDbImplementation.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the BaseDbImplementation.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Dispose managed resources if any
            _disposed = true;
        }
    }
    #endregion
}
