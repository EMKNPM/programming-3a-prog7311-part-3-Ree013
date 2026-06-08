using Microsoft.AspNetCore.Mvc;
using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;
using TechMovePrototype.Services;

namespace TechMovePrototype.Controllers;

public class ContractsController : Controller
{
    private readonly IApiService _api;
    private readonly IWebHostEnvironment _env;

    public ContractsController(IApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, ContractStatus? status)
    {
        var queryParts = new List<string>();
        if (fromDate.HasValue) queryParts.Add($"fromDate={fromDate:yyyy-MM-dd}");
        if (toDate.HasValue) queryParts.Add($"toDate={toDate:yyyy-MM-dd}");
        if (status.HasValue) queryParts.Add($"status={status}");

        var qs = queryParts.Any() ? "?" + string.Join("&", queryParts) : "";

        var contracts = await _api.GetAsync<List<Contract>>($"api/contracts{qs}")
                        ?? new List<Contract>();

        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;

        return View(contracts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _api.GetAsync<Contract>($"api/contracts/{id}");
        if (contract == null) return NotFound();
        return View(contract);
    }

    public async Task<IActionResult> Create()
    {
        var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clients, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract,
        IFormFile? signedAgreement)
    {
        string? savedFilePath = null;

        if (signedAgreement != null && signedAgreement.Length > 0)
        {
            if (!signedAgreement.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("signedAgreement", "Only PDF files are allowed.");
            }
            else
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(signedAgreement.FileName)}";
                var fullPath = Path.Combine(folder, fileName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                await signedAgreement.CopyToAsync(stream);

              
                savedFilePath = Path.Combine("uploads", "contracts", fileName);
            }
        }

        if (ModelState.IsValid)
        {
            var created = await _api.PostAsync<Contract>("api/contracts", new
            {
                contract.ClientId,
                contract.StartDate,
                contract.EndDate,
                Status = contract.Status.ToString(),
                contract.ServiceLevel,
                SignedAgreementFilePath = savedFilePath  
            });

            if (created != null)
            {
                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create contract. Please check the API is running.");
        }

        var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clients, "Id", "Name");
        return View(contract);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _api.GetAsync<Contract>($"api/contracts/{id}");
        if (contract == null) return NotFound();

        var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            clients, "Id", "Name", contract.ClientId);
        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract)
    {
        if (ModelState.IsValid)
        {
            var updated = await _api.PutAsync<Contract>($"api/contracts/{id}", new
            {
                contract.ClientId,
                contract.StartDate,
                contract.EndDate,
                Status = contract.Status.ToString(),
                contract.ServiceLevel
            });

            if (updated != null)
            {
                TempData["Success"] = "Contract updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update contract. Please try again.");
        }

        var clients = await _api.GetAsync<List<Client>>("api/clients") ?? new List<Client>();
        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            clients, "Id", "Name", contract.ClientId);
        return View(contract);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, ContractStatus status)
    {
        await _api.PatchAsync($"api/contracts/{id}/status", new { Status = status.ToString() });
        TempData["Success"] = "Status updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var contract = await _api.GetAsync<Contract>($"api/contracts/{id}");
        if (contract == null) return NotFound();
        return View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteAsync($"api/contracts/{id}");
        TempData["Success"] = "Contract deleted.";
        return RedirectToAction(nameof(Index));
    }

    
    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _api.GetAsync<Contract>($"api/contracts/{id}");
        if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementFilePath))
            return NotFound();

        var fullPath = Path.Combine(
            _env.WebRootPath,
            contract.SignedAgreementFilePath
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(bytes, "application/pdf", Path.GetFileName(fullPath));
    }
}