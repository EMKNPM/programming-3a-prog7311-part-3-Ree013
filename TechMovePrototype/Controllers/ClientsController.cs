using Microsoft.AspNetCore.Mvc;
using TechMovePrototype.Models;
using TechMovePrototype.Services;

namespace TechMovePrototype.Controllers;

public class ClientsController : Controller
{
    private readonly IApiService _api;

    public ClientsController(IApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var clients = await _api.GetAsync<List<Client>>("api/clients")
                      ?? new List<Client>();
        return View(clients);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = await _api.GetAsync<Client>($"api/clients/{id}");
        if (client == null) return NotFound();
        return View(client);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,ContactEmail,ContactPhone,Region")] Client client)
    {
        if (ModelState.IsValid)
        {
            var created = await _api.PostAsync<Client>("api/clients", new
            {
                client.Name,
                client.ContactEmail,
                client.ContactPhone,
                client.Region
            });

            if (created != null)
            {
                TempData["Success"] = "Client created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create client. Please try again.");
        }

        return View(client);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var client = await _api.GetAsync<Client>($"api/clients/{id}");
        if (client == null) return NotFound();
        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Name,ContactEmail,ContactPhone,Region")] Client client)
    {
        if (ModelState.IsValid)
        {
            var updated = await _api.PutAsync<Client>($"api/clients/{id}", new
            {
                client.Name,
                client.ContactEmail,
                client.ContactPhone,
                client.Region
            });

            if (updated != null)
            {
                TempData["Success"] = "Client updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update client. Please try again.");
        }

        return View(client);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var client = await _api.GetAsync<Client>($"api/clients/{id}");
        if (client == null) return NotFound();
        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteAsync($"api/clients/{id}");
        TempData["Success"] = "Client deleted.";
        return RedirectToAction(nameof(Index));
    }
}