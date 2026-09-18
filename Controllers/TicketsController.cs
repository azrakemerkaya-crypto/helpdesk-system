using System.Security.Claims;
using HelpdeskSystem.Business.Services;
using HelpdeskSystem.Data;
using HelpdeskSystem.DTOs;
using HelpdeskSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Controllers;

[ApiController, Route("api/tickets"), Authorize]
public class TicketsController(AppDbContext db, ITicketService ticketService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsStaff => User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.Technician));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TicketStatus? status, [FromQuery] TicketPriority? priority)
    {
        var tickets = await ticketService.GetTicketsAsync(UserId, IsStaff, status, priority);
        return Ok(tickets.Select(x => new
        {
            x.Id,
            x.Title,
            x.Description,
            x.Status,
            x.Priority,
            category = x.Category?.Name,
            createdBy = x.CreatedBy?.FullName,
            assignedTo = x.AssignedTo?.FullName,
            x.CreatedAt,
            x.UpdatedAt,
            x.DeviceSerialNumber
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var ticket = await db.Tickets
            .Include(x => x.Category)
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
            .Include(x => x.Comments)
            .ThenInclude(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (ticket is null || (!IsStaff && ticket.CreatedById != UserId))
            return NotFound();

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        var ticket = await ticketService.CreateTicketAsync(UserId, request);
        if (ticket is null)
            return BadRequest("Geçerli bir kategori seçilmelidir.");

        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var ticket = await ticketService.UpdateStatusAsync(id, UserId, IsStaff, request.Status);
        if (ticket is null)
            return NotFound();

        return Ok(ticket);
    }

    [HttpPut("{id:int}/assign")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Assign(int id, AssignTicketRequest request)
    {
        var ticket = await ticketService.AssignTicketAsync(id, request.TechnicianId);
        if (ticket is null)
            return NotFound("Talep veya teknik personel bulunamadı.");

        return Ok(ticket);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, AddCommentRequest request)
    {
        var comment = await ticketService.AddCommentAsync(id, UserId, IsStaff, request.Text);
        if (comment is null)
            return NotFound();

        return Ok(comment);
    }
}
