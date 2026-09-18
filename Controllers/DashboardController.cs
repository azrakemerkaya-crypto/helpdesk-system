using HelpdeskSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Controllers;

[ApiController, Route("api/dashboard"), Authorize(Roles = "Admin,Technician")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary() => Ok(new
    {
        total = await db.Tickets.CountAsync(),
        open = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Open),
        inProgress = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.InProgress),
        resolved = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Resolved),
        closed = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Closed)
    });
}
