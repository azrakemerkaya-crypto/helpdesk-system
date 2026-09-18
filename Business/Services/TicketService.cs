using HelpdeskSystem.Data;
using HelpdeskSystem.DTOs;
using HelpdeskSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Business.Services;

public interface ITicketService
{
    Task<List<Ticket>> GetTicketsAsync(int userId, bool isStaff, TicketStatus? status, TicketPriority? priority);
    Task<Ticket?> CreateTicketAsync(int userId, CreateTicketRequest request);
    Task<Ticket?> UpdateStatusAsync(int ticketId, int userId, bool isStaff, TicketStatus status);
    Task<Ticket?> AssignTicketAsync(int ticketId, int technicianId);
    Task<Comment?> AddCommentAsync(int ticketId, int userId, bool isStaff, string text);
}

public class TicketService(AppDbContext db) : ITicketService
{
    public async Task<List<Ticket>> GetTicketsAsync(int userId, bool isStaff, TicketStatus? status, TicketPriority? priority)
    {
        var query = db.Tickets
            .Include(x => x.Category)
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
            .AsQueryable();

        if (!isStaff)
            query = query.Where(x => x.CreatedById == userId);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(x => x.Priority == priority.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<Ticket?> CreateTicketAsync(int userId, CreateTicketRequest request)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.IsActive))
            return null;

        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            Priority = request.Priority,
            DeviceSerialNumber = request.DeviceSerialNumber,
            CreatedById = userId,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket?> UpdateStatusAsync(int ticketId, int userId, bool isStaff, TicketStatus status)
    {
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null || (!isStaff && ticket.CreatedById != userId))
            return null;

        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (status == TicketStatus.Closed)
            ticket.ClosedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket?> AssignTicketAsync(int ticketId, int technicianId)
    {
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null)
            return null;

        var technician = await db.Users.SingleOrDefaultAsync(x => x.Id == technicianId && x.Role == UserRole.Technician);
        if (technician is null)
            return null;

        ticket.AssignedToId = technician.Id;
        ticket.Status = TicketStatus.Assigned;
        ticket.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return ticket;
    }

    public async Task<Comment?> AddCommentAsync(int ticketId, int userId, bool isStaff, string text)
    {
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null || (!isStaff && ticket.CreatedById != userId))
            return null;

        var comment = new Comment
        {
            TicketId = ticketId,
            UserId = userId,
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return comment;
    }
}
