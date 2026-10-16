using CrmApi.Data;
using CrmApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmApi.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController(AppDbContext db) : ControllerBase
{
    public record CustomerDto(string Name, string? Email, string? Phone, string? Company);

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await db.Customers.OrderBy(c => c.Name).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });
        var c = new Customer
        {
            Name = dto.Name.Trim(),
            Email = dto.Email ?? "",
            Phone = dto.Phone ?? "",
            Company = dto.Company ?? ""
        };
        db.Customers.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await db.Customers.Include(x => x.Deals).SingleOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        return Ok(c);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CustomerDto dto)
    {
        var c = await db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });
        c.Name = dto.Name.Trim();
        c.Email = dto.Email ?? "";
        c.Phone = dto.Phone ?? "";
        c.Company = dto.Company ?? "";
        await db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        db.Customers.Remove(c);
        return Ok(new { ok = true });
    }
}
