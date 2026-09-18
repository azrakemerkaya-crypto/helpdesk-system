using System.Security.Claims;
using HelpdeskSystem.Data;
using HelpdeskSystem.DTOs;
using HelpdeskSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Controllers;

[ApiController, Route("api/tickets"), Authorize]
public class TicketsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsStaff => User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.Technician));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TicketStatus? status, [FromQuery] TicketPriority? priority)
    {
        var query = db.Tickets.AsNoTracking().Include(x => x.Category).Include(x => x.CreatedBy).Include(x => x.AssignedTo).AsQueryable();
        if (!IsStaff) query = query.Where(x => x.CreatedById == UserId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (priority.HasValue) query = query.Where(x => x.Priority == priority);
        return Ok(await query.OrderByDescending(x => x.CreatedAt).Select(x => new { x.Id, x.Title, x.Description, x.Status, x.Priority, category = x.Category!.Name, createdBy = x.CreatedBy!.FullName, assignedTo = x.AssignedTo == null ? null : x.AssignedTo.FullName, x.CreatedAt, x.UpdatedAt }).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var ticket = await db.Tickets.AsNoTracking().Include(x => x.Category).Include(x => x.CreatedBy).Include(x => x.AssignedTo).Include(x => x.Comments).ThenInclude(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
        if (ticket is null || (!IsStaff && ticket.CreatedById != UserId)) return NotFound();
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.IsActive)) return BadRequest("Geçerli bir kategori seçilmelidir.");
        var ticket = new Ticket { Title = request.Title, Description = request.Description, CategoryId = request.CategoryId, Priority = request.Priority, DeviceSerialNumber = request.DeviceSerialNumber, CreatedById = UserId };
        db.Tickets.Add(ticket); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket is null || (!IsStaff && ticket.CreatedById != UserId)) return NotFound();
        ticket.Status = request.Status; ticket.UpdatedAt = DateTime.UtcNow; if (request.Status == TicketStatus.Closed) ticket.ClosedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(); return Ok(ticket);
    }

    [HttpPut("{id:int}/assign"), Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Assign(int id, AssignTicketRequest request)
    {
        var ticket = await db.Tickets.FindAsync(id); var technician = await db.Users.SingleOrDefaultAsync(x => x.Id == request.TechnicianId && x.Role == UserRole.Technician);
        if (ticket is null || technician is null) return NotFound("Talep veya teknik personel bulunamadı.");
        ticket.AssignedToId = technician.Id; ticket.Status = TicketStatus.Assigned; ticket.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(ticket);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, AddCommentRequest request)
    {
        var ticket = await db.Tickets.FindAsync(id); if (ticket is null || (!IsStaff && ticket.CreatedById != UserId)) return NotFound();
        var comment = new Comment { TicketId = id, UserId = UserId, Text = request.Text }; db.Comments.Add(comment); await db.SaveChangesAsync(); return Ok(comment);
    }
}
