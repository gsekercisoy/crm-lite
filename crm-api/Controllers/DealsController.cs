using System.Security.Claims;
using CrmApi.Data;
using CrmApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmApi.Controllers;

[ApiController]
[Route("api/deals")]
[Authorize]
public class DealsController(AppDbContext db) : ControllerBase
{
    public record CreateDealDto(int CustomerId, string Title, decimal Value, string? Stage, DateTime? ExpectedClose);
    public record StageDto(string Stage);
    public record ActivityDto(string Kind, string Text);

    private int CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? stage)
    {
        var q = db.Deals.Include(d => d.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(stage))
            q = q.Where(d => d.Stage == stage);
        return Ok(await q.OrderByDescending(d => d.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var d = await db.Deals.Include(x => x.Customer).Include(x => x.Activities).SingleOrDefaultAsync(x => x.Id == id);
        if (d is null) return NotFound();
        return Ok(d);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDealDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { error = "Title is required." });
        var stage = string.IsNullOrWhiteSpace(dto.Stage) ? DealStages.Lead : dto.Stage;
        if (!DealStages.IsValid(stage))
            return BadRequest(new { error = "Invalid stage." });
        if (!await db.Customers.AnyAsync(c => c.Id == dto.CustomerId))
            return BadRequest(new { error = "Customer not found." });
        var deal = new Deal
        {
            CustomerId = dto.CustomerId,
            OwnerId = CurrentUserId(),
            Title = dto.Title.Trim(),
            Value = dto.Value,
            Stage = stage,
            ExpectedClose = dto.ExpectedClose,
            CreatedAt = DateTime.UtcNow
        };
        db.Deals.Add(deal);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = deal.Id }, deal);
    }

    [HttpPost("{id:int}/stage")]
    public async Task<IActionResult> ChangeStage(int id, StageDto dto)
    {
        if (!DealStages.IsValid(dto.Stage))
            return BadRequest(new { error = "Invalid stage." });
        var deal = await db.Deals.FindAsync(id);
        if (deal is null) return NotFound();
        if (deal.IsClosed)
            return BadRequest(new { error = $"Closed deal (stage={deal.Stage}) cannot change stage." });
        deal.Stage = dto.Stage;
        await db.SaveChangesAsync();
        return Ok(deal);
    }

    [HttpGet("{id:int}/activities")]
    public async Task<IActionResult> ListActivities(int id)
    {
        if (!await db.Deals.AnyAsync(d => d.Id == id)) return NotFound();
        var items = await db.Activities.Where(a => a.DealId == id)
            .OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Ok(items);
    }

    [HttpPost("{id:int}/activities")]
    public async Task<IActionResult> AddActivity(int id, ActivityDto dto)
    {
        if (!await db.Deals.AnyAsync(d => d.Id == id)) return NotFound();
        if (!ActivityKinds.IsValid(dto.Kind))
            return BadRequest(new { error = "Invalid kind. Use Call/Meeting/Email/Note." });
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new { error = "Text is required." });
        var a = new Activity
        {
            DealId = id,
            UserId = CurrentUserId(),
            Kind = dto.Kind,
            Text = dto.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.Activities.Add(a);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(ListActivities), new { id }, a);
    }
}
