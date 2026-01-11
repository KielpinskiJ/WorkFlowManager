using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Data;
using WorkFlowManager.Models;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Controllers;

/// <summary>
/// Controller for managing employee bonuses. Admin only.
/// </summary>
[Authorize(Roles = "Admin")]
public class BonusesController : Controller
{
    private const int PageSizeBuffer = 5;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BonusesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Displays paginated list of all bonuses.
    /// </summary>
    /// <param name="page"></param>
    public async Task<IActionResult> Index(int page = 1)
    {
        if (page < 1) page = 1;

        //total employee count + buffer
        var employeeCount = await _context.Users.CountAsync();
        var pageSize = employeeCount + PageSizeBuffer;

        var totalCount = await _context.Bonuses.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page > totalPages && totalPages > 0) page = totalPages;

        var bonuses = await _context.Bonuses
            .Include(b => b.User)
            .OrderByDescending(b => b.DateGranted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var allBonusesStats = await _context.Bonuses
            .GroupBy(_ => 1)
            .Select(g => new 
            { 
                TotalAmount = g.Sum(b => b.Amount),
                Count = g.Count(),
                AvgAmount = g.Average(b => b.Amount)
            })
            .FirstOrDefaultAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalBonusAmount = allBonusesStats?.TotalAmount ?? 0;
        ViewBag.BonusCount = allBonusesStats?.Count ?? 0;
        ViewBag.AvgBonusAmount = allBonusesStats?.AvgAmount ?? 0;

        return View(bonuses);
    }

    // GET: Bonuses/Create
    public async Task<IActionResult> Create()
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var model = new CreateBonusViewModel
        {
            Users = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.Email})"
            })
        };

        return View(model);
    }

    // POST: Bonuses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBonusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Repopulate users dropdown
            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            model.Users = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.Email})"
            });

            return View(model);
        }

        var bonus = new Bonus
        {
            UserId = model.UserId,
            Amount = model.Amount,
            Reason = model.Reason,
            DateGranted = model.DateGranted
        };

        _context.Bonuses.Add(bonus);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Bonus added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Bonuses/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var bonus = await _context.Bonuses
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bonus == null)
        {
            return NotFound();
        }

        return View(bonus);
    }

    // GET: Bonuses/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var bonus = await _context.Bonuses
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bonus == null)
        {
            return NotFound();
        }

        return View(bonus);
    }

    // POST: Bonuses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bonus = await _context.Bonuses.FindAsync(id);
        if (bonus == null)
        {
            return NotFound();
        }

        _context.Bonuses.Remove(bonus);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Bonus deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

