using CrmApi.Data;
using CrmApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmApi.Controllers;

[ApiController]
[Route("api/pipeline")]
[Authorize]
public class PipelineController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var deals = await db.Deals.ToListAsync();
        var stages = DealStages.All.Select(s => new
        {
            stage = s,
            count = deals.Count(d => d.Stage == s),
            total = deals.Where(d => d.Stage == s).Sum(d => d.Value)
        });
        return Ok(new
        {
            stages,
            totalCount = deals.Count,
            totalValue = deals.Sum(d => d.Value)
        });
    }
}
