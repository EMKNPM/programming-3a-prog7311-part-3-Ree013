using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMovePrototype.Builders;
using TechMovePrototype.Data;
using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;
using TechMovePrototype.Observers;
using TechMovePrototype.Services;

namespace TechMovePrototype.Controllers;

public class ContractsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IValidationService _validationService;

    public ContractsController(AppDbContext context, IWebHostEnvironment env, IValidationService validationService)
    {
        _context = context;
        _env = env;
        _validationService = validationService;
    }

    // GET: Contracts
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, ContractStatus? status)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(c => c.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.EndDate <= toDate.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;

        return View(await query.ToListAsync());
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (contract == null) return NotFound();

        return View(contract);
    }

    // GET: Contracts/Create
    public IActionResult Create()
    {
        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _context.Clients.OrderBy(c => c.Name), "Id", "Name");

        return View();
    }

    // POST: Contracts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
    {
        if (!_validationService.ValidateContract(contract, out var errors))
        {
            foreach (var error in errors)
                ModelState.AddModelError("", error);
        }

        if (signedAgreement == null || signedAgreement.Length == 0)
        {
            ModelState.AddModelError("signedAgreement", "Signed Agreement PDF is required.");
        }
        else if (!signedAgreement.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("signedAgreement", "Only PDF files are allowed.");
        }

        if (ModelState.IsValid)
        {
           
            var contractToSave = new ContractBuilder()
                .WithClient(contract.ClientId!.Value)
                .WithDates(contract.StartDate, contract.EndDate)
                .WithStatus(contract.Status)
                .WithServiceLevel(contract.ServiceLevel)
                .Build();

           
            if (signedAgreement != null && signedAgreement.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(signedAgreement.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await signedAgreement.CopyToAsync(stream);

                contractToSave.SignedAgreementFilePath = $"/uploads/contracts/{fileName}";
            }

            _context.Contracts.Add(contractToSave);
            await _context.SaveChangesAsync();

      
            var observer = HttpContext.RequestServices.GetRequiredService<IContractObserver>();
            contractToSave.Attach(observer);

            TempData["Success"] = "Contract created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _context.Clients.OrderBy(c => c.Name), "Id", "Name");

        return View(contract);
    }

    // GET: Contracts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _context.Clients.OrderBy(c => c.Name),
            "Id",
            "Name",
            contract.ClientId);

        return View(contract);
    }

    // POST: Contracts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,SignedAgreementFilePath")] Contract contract, IFormFile? signedAgreement)
    {
        if (id != contract.Id) return NotFound();

        if (signedAgreement != null && signedAgreement.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(signedAgreement.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await signedAgreement.CopyToAsync(stream);

            contract.SignedAgreementFilePath = $"/uploads/contracts/{fileName}";
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(contract.Id)) return NotFound();
                throw;
            }
        }

        ViewBag.Clients = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _context.Clients.OrderBy(c => c.Name),
            "Id",
            "Name",
            contract.ClientId);

        return View(contract);
    }

    // GET: Contracts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (contract == null) return NotFound();

        return View(contract);
    }

    // POST: Contracts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract != null)
        {
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

   
    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract?.SignedAgreementFilePath == null) return NotFound();

        var filePath = Path.Combine(_env.WebRootPath, contract.SignedAgreementFilePath.TrimStart('/'));
        if (!System.IO.File.Exists(filePath)) return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
    }

    private bool ContractExists(int id)
    {
        return _context.Contracts.Any(e => e.Id == id);
    }
}