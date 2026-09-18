using HelpdeskSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Business.Services;

public interface IDashboardService
{
    Task<object> GetSummaryAsync();
}

public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<object> GetSummaryAsync()
    {
        return new
        {
            total = await db.Tickets.CountAsync(),
            open = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Open),
            assigned = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Assigned),
            inProgress = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.InProgress),
            resolved = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Resolved),
            closed = await db.Tickets.CountAsync(x => x.Status == Models.TicketStatus.Closed)
        };
    }
}
