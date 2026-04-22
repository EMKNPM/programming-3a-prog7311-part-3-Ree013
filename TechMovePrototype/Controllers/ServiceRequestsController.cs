using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using TechMovePrototype.Data;
using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public ServiceRequestsController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // GET: ServiceRequests
    public async Task<IActionResult> Index()
    {
        var requests = await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .OrderByDescending(s => s.Id)
            .ToListAsync();

        return View(requests);
    }

    // GET: ServiceRequests/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var serviceRequest = await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serviceRequest == null) return NotFound();

        return View(serviceRequest);
    }

    // GET: ServiceRequests/Create?contractId=5
    public async Task<IActionResult> Create(int contractId)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == contractId);

        if (contract == null) return NotFound();

        if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
        {
            TempData["Error"] = "Cannot create Service Request for an Expired or On Hold contract.";
            return RedirectToAction("Index", "Contracts");
        }

        ViewBag.ContractId = contractId;
        ViewBag.ContractInfo = $"{contract.Client.Name} - {contract.ServiceLevel}";

        decimal rate = 18.5m;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://open.er-api.com/v6/latest/USD");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data?.Rates != null && data.Rates.TryGetValue("ZAR", out var zarRate))
                    rate = zarRate;
            }
        }
        catch { }

        ViewBag.UsdToZarRate = rate;

        return View();
    }

    // POST: ServiceRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        int contractId = 0;
        decimal usdValue = 0;
        decimal zarValue = 0;
        string description = Request.Form["Description"].ToString().Trim();
        ServiceRequestStatus status = ServiceRequestStatus.Pending;

        int.TryParse(Request.Form["ContractId"], out contractId);
        decimal.TryParse(Request.Form["USDValue"], NumberStyles.Any, CultureInfo.InvariantCulture, out usdValue);
        decimal.TryParse(Request.Form["ZARCost"], NumberStyles.Any, CultureInfo.InvariantCulture, out zarValue);

        if (Enum.TryParse(Request.Form["Status"], out ServiceRequestStatus parsedStatus))
            status = parsedStatus;

        if (contractId <= 0)
            ModelState.AddModelError("ContractId", "The Contract field is required.");

        if (usdValue <= 0)
            ModelState.AddModelError("USDValue", "USD Amount must be greater than 0");

        if (string.IsNullOrWhiteSpace(description))
            ModelState.AddModelError("Description", "Description is required.");

        if (ModelState.IsValid)
        {
            var newRequest = new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                USDValue = usdValue,
                ZARCost = zarValue,
                Status = status
            };

            _context.ServiceRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service Request created successfully!";
            return RedirectToAction("Index", "Contracts");
        }

        ViewBag.ContractId = contractId;
        ViewBag.UsdToZarRate = 18.5m;

        return View();
    }

    // GET: ServiceRequests/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var serviceRequest = await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serviceRequest == null) return NotFound();

        return View(serviceRequest);
    }

    // POST: ServiceRequests/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        int contractId = 0;
        decimal usdValue = 0;
        decimal zarValue = 0;
        string description = Request.Form["Description"].ToString().Trim();
        ServiceRequestStatus status = ServiceRequestStatus.Pending;

        int.TryParse(Request.Form["ContractId"], out contractId);
        decimal.TryParse(Request.Form["USDValue"], NumberStyles.Any, CultureInfo.InvariantCulture, out usdValue);
        decimal.TryParse(Request.Form["ZARCost"], NumberStyles.Any, CultureInfo.InvariantCulture, out zarValue);

        if (Enum.TryParse(Request.Form["Status"], out ServiceRequestStatus parsedStatus))
            status = parsedStatus;

        if (contractId <= 0)
            ModelState.AddModelError("ContractId", "The Contract field is required.");

        if (usdValue <= 0)
            ModelState.AddModelError("USDValue", "USD Amount must be greater than 0");

        if (string.IsNullOrWhiteSpace(description))
            ModelState.AddModelError("Description", "Description is required.");

        if (ModelState.IsValid)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null) return NotFound();

            serviceRequest.ContractId = contractId;
            serviceRequest.Description = description;
            serviceRequest.USDValue = usdValue;
            serviceRequest.ZARCost = zarValue;
            serviceRequest.Status = status;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating: {ex.Message}");
            }
        }

        
        var reloaded = await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(s => s.Id == id);

        return View(reloaded ?? new ServiceRequest());
    }

    // GET: ServiceRequests/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var serviceRequest = await _context.ServiceRequests
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serviceRequest == null) return NotFound();

        return View(serviceRequest);
    }

    // POST: ServiceRequests/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);
        if (serviceRequest != null)
        {
            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}