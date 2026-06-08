using Microsoft.AspNetCore.Mvc;
using GLMS.API.Models;
using GLMS.API.Services;


namespace GLMS.API.Controllers;

[ApiController]
[Route("api/contracts")]

public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] ContractStatus? status)
    {
        var contracts = await _contractService.GetAllAsync(fromDate, toDate, status);
        return Ok(contracts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var contract = await _contractService.GetByIdAsync(id);
        if (contract == null)
            return NotFound(new { message = $"Contract {id} not found." });
        return Ok(contract);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContractDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _contractService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateContractDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _contractService.UpdateAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = $"Contract {id} not found." });
        return Ok(updated);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateContractStatusDto dto)
    {
        var updated = await _contractService.UpdateStatusAsync(id, dto.Status);
        if (updated == null)
            return NotFound(new { message = $"Contract {id} not found." });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _contractService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Contract {id} not found." });
        return NoContent();
    }
}