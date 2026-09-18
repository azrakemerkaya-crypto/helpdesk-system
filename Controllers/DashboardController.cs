using HelpdeskSystem.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskSystem.Controllers;

[ApiController, Route("api/dashboard"), Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var summary = await dashboardService.GetSummaryAsync();
        return Ok(summary);
    }
}
