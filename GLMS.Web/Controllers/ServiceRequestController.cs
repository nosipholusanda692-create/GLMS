using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly GLMSDbContext _context;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestController(GLMSDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .ToListAsync();

            return View(requests);
        }

        // GET: ServiceRequest/Create
        // This loads the page and fetches the live exchange rate to show the user
        public async Task<IActionResult> Create()
        {
            // Only show ACTIVE contracts - workflow logic from Part 1 Zachman rules
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(activeContracts, "Id", "Title");

            // Get current exchange rate to display on the form
            var rate = await _currencyService.GetUSDToZARRateAsync();
            ViewBag.CurrentRate = rate;

            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            // Workflow validation - cannot create against Expired or OnHold contracts
            // This implements the Observer/business rule from our Part 1 design
            var contract = await _context.Contracts.FindAsync(request.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("", "Contract not found.");
            }
            else if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("ContractId",
                    $"Cannot create a service request for a contract that is '{contract.Status}'. Only Active contracts are allowed.");
            }

            if (ModelState.IsValid)
            {
                // Call the currency API and calculate ZAR cost (Strategy pattern from Part 1)
                var rate = await _currencyService.GetUSDToZARRateAsync();
                request.CostZAR = _currencyService.ConvertUSDToZAR(request.CostUSD, rate);
                request.ExchangeRateUsed = rate;
                request.CreatedDate = DateTime.Now;

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Service request created! USD {request.CostUSD:F2} = ZAR {request.CostZAR:F2} (rate: {rate:F4})";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(activeContracts, "Id", "Title");
            ViewBag.CurrentRate = await _currencyService.GetUSDToZARRateAsync();

            return View(request);
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        // GET: ServiceRequest/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        // POST: ServiceRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request != null)
            {
                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service request deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoint - called from JS when user types a USD amount
        [HttpGet]
        public async Task<IActionResult> GetZARAmount(decimal usdAmount)
        {
            var rate = await _currencyService.GetUSDToZARRateAsync();
            var zarAmount = _currencyService.ConvertUSDToZAR(usdAmount, rate);
            return Json(new { zarAmount = zarAmount.ToString("F2"), rate = rate.ToString("F4") });
        }
    }
}
