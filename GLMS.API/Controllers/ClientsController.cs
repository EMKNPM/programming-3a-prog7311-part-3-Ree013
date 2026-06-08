using Microsoft.AspNetCore.Mvc;
using GLMS.API.Data;
using GLMS.API.Models;
using Microsoft.EntityFrameworkCore;


namespace GLMS.API.Controllers;

[ApiController]
[Route("api/clients")]

public class ClientsController : ControllerBase
{
    private readonly ApiDbContext _db;

    public ClientsController(ApiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _db.Clients.ToListAsync();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return NotFound(new { message = $"Client {id} not found." });
        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client client)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Client client)
    {
        var existing = await _db.Clients.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Client {id} not found." });

        existing.Name = client.Name;
        existing.ContactEmail = client.ContactEmail;
        existing.ContactPhone = client.ContactPhone;
        existing.Region = client.Region;

        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return NotFound(new { message = $"Client {id} not found." });

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}