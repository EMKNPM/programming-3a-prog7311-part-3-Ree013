using Microsoft.AspNetCore.Mvc;
using GLMS.API.Models;
using GLMS.API.Services;


namespace GLMS.API.Controllers;

[ApiController]
[Route("api/servicerequests")]

public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _service;

    public ServiceRequestsController(IServiceRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _service.GetByIdAsync(id);
        return r == null ? NotFound(new { message = $"Service Request {id} not found." }) : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = $"Service Request {id} not found." });
        return Ok(updated);
    }
    [HttpDelete("{id}")]  
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Service Request {id} not found." });

        return NoContent();
    }
}
