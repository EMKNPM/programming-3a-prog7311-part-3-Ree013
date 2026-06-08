using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;
using TechMovePrototype.Services;


namespace TechMovePrototype.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly IApiService _api;
    private readonly IHttpClientFactory _httpClientFactory;

    public ServiceRequestsController(IApiService api, IHttpClientFactory httpClientFactory)
    {
        _api = api;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _api.GetAsync<List<ServiceRequest>>("api/servicerequests")
                       ?? new List<ServiceRequest>();
        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _api.GetAsync<ServiceRequest>($"api/servicerequests/{id}");
        if (request == null) return NotFound();
        return View(request);
    }

    public async Task<IActionResult> Create(int contractId)
    {
        var contract = await _api.GetAsync<Contract>($"api/contracts/{contractId}");
        if (contract == null) return NotFound();

        if (contract.Status == ContractStatus.Expired ||
            contract.Status == ContractStatus.OnHold)
        {
            TempData["Error"] = "Cannot create a Service Request for an Expired or On Hold contract.";
            return RedirectToAction("Index", "Contracts");
        }

        ViewBag.ContractId = contractId;
        ViewBag.ContractInfo = $"{contract.Client?.Name} - {contract.ServiceLevel}";
        ViewBag.UsdToZarRate = await FetchUsdToZarRateAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        int.TryParse(Request.Form["ContractId"], out int contractId);
        decimal.TryParse(Request.Form["USDValue"], NumberStyles.Any,
            CultureInfo.InvariantCulture, out decimal usdValue);
        decimal.TryParse(Request.Form["ZARCost"], NumberStyles.Any,
            CultureInfo.InvariantCulture, out decimal zarValue);

        string description = Request.Form["Description"].ToString().Trim();

        if (contractId <= 0)
            ModelState.AddModelError("ContractId", "Contract is required.");
        if (usdValue <= 0)
            ModelState.AddModelError("USDValue", "USD Amount must be greater than 0.");
        if (string.IsNullOrWhiteSpace(description))
            ModelState.AddModelError("Description", "Description is required.");

        if (ModelState.IsValid)
        {
            var created = await _api.PostAsync<ServiceRequest>("api/servicerequests", new
            {
                ContractId = contractId,
                Description = description,
                USDValue = usdValue,
                ZARCost = zarValue
            });

            if (created != null)
            {
                TempData["Success"] = "Service Request created successfully!";
                return RedirectToAction("Index", "Contracts");
            }

            ModelState.AddModelError("", "Failed to create request. The contract may be Expired or On Hold.");
        }

        ViewBag.ContractId = contractId;
        ViewBag.UsdToZarRate = 18.5m;

        if (contractId > 0)
        {
            var contract = await _api.GetAsync<Contract>($"api/contracts/{contractId}");
            ViewBag.ContractInfo = contract != null
                ? $"{contract.Client?.Name} - {contract.ServiceLevel}"
                : "";
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var request = await _api.GetAsync<ServiceRequest>($"api/servicerequests/{id}");
        if (request == null) return NotFound();

        ViewBag.UsdToZarRate = await FetchUsdToZarRateAsync();
        return View(request);
    }

    [HttpPost]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id)
    {
        int.TryParse(Request.Form["ContractId"], out int contractId);
        decimal.TryParse(Request.Form["USDValue"], NumberStyles.Any,
            CultureInfo.InvariantCulture, out decimal usdValue);
        decimal.TryParse(Request.Form["ZARCost"], NumberStyles.Any,
            CultureInfo.InvariantCulture, out decimal zarValue);

        string description = Request.Form["Description"].ToString().Trim();
        string statusStr = Request.Form["Status"].ToString();
        Enum.TryParse<TechMovePrototype.Models.Enums.ServiceRequestStatus>(statusStr, out var status);

        if (usdValue <= 0)
            ModelState.AddModelError("USDValue", "USD Amount must be greater than 0.");
        if (string.IsNullOrWhiteSpace(description))
            ModelState.AddModelError("Description", "Description is required.");

        if (ModelState.IsValid)
        {
            var updated = await _api.PutAsync<ServiceRequest>($"api/servicerequests/{id}", new
            {
                Description = description,
                USDValue = usdValue,
                ZARCost = zarValue,
                Status = statusStr
            });

            if (updated != null)
            {
                TempData["Success"] = "Service Request updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update service request. Please try again.");
        }

        var request = await _api.GetAsync<ServiceRequest>($"api/servicerequests/{id}")
                      ?? new ServiceRequest { Id = id };
        ViewBag.UsdToZarRate = 18.5m;
        return View(request);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var request = await _api.GetAsync<ServiceRequest>($"api/servicerequests/{id}");
        if (request == null) return NotFound();
        return View(request);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteAsync($"api/servicerequests/{id}");
        TempData["Success"] = "Service Request deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<decimal> FetchUsdToZarRateAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data?.Rates != null && data.Rates.TryGetValue("ZAR", out var zarRate))
                    return zarRate;
            }
        }
        catch { /* fall through to default */ }

        return 18.5m;
    }
}

