using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ContractController : Controller
    {
        private readonly GLMSDbContext _context;
        private readonly IFileService _fileService;

        public ContractController(GLMSDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Contract — with search and filter using LINQ
        public async Task<IActionResult> Index(string? statusFilter, DateTime? startFrom, DateTime? startTo)
        {
            // Start with all contracts including client name
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            // LINQ filter by status
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ContractStatus>(statusFilter, out var status))
            {
                query = query.Where(c => c.Status == status);
            }

            // LINQ filter by date range
            if (startFrom.HasValue)
            {
                query = query.Where(c => c.StartDate >= startFrom.Value);
            }

            if (startTo.HasValue)
            {
                query = query.Where(c => c.StartDate <= startTo.Value);
            }

            // Pass filter values back to view so the form stays filled in
            ViewBag.StatusFilter = statusFilter;
            ViewBag.StartFrom = startFrom?.ToString("yyyy-MM-dd");
            ViewBag.StartTo = startTo?.ToString("yyyy-MM-dd");
            ViewBag.StatusOptions = Enum.GetNames(typeof(ContractStatus));

            var contracts = await query.ToListAsync();
            return View(contracts);
        }

        // GET: Contract/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // GET: Contract/Create
        public IActionResult Create()
        {
            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name");
            return View();
        }

        // POST: Contract/Create — handles PDF upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? signedAgreement)
        {
            // Handle the PDF file upload
            if (signedAgreement != null)
            {
                // Validate: only PDF allowed (this logic is also unit tested)
                if (!_fileService.IsValidPdf(signedAgreement))
                {
                    ModelState.AddModelError("SignedAgreementPath", "Only PDF files are allowed. Max size is 10MB.");
                    ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name");
                    return View(contract);
                }

                // Save the file and store the path
                contract.SignedAgreementPath = await _fileService.SaveFileAsync(signedAgreement);
            }

            if (ModelState.IsValid)
            {
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name");
            return View(contract);
        }

        // GET: Contract/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contract/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? signedAgreement)
        {
            if (id != contract.Id)
                return NotFound();

            // Handle new PDF upload on edit
            if (signedAgreement != null)
            {
                if (!_fileService.IsValidPdf(signedAgreement))
                {
                    ModelState.AddModelError("SignedAgreementPath", "Only PDF files are allowed.");
                    ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
                    return View(contract);
                }
                contract.SignedAgreementPath = await _fileService.SaveFileAsync(signedAgreement);
            }

            if (ModelState.IsValid)
            {
                _context.Update(contract);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract updated!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contract/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // POST: Contract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Download the signed PDF
        public IActionResult Download(int id)
        {
            var contract = _context.Contracts.Find(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", contract.SignedAgreementPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", contract.SignedAgreementPath);
        }
    }
}
