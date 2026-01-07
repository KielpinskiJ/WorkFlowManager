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
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BonusesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Bonuses
    public async Task<IActionResult> Index()
    {
        var bonuses = await _context.Bonuses
            .Include(b => b.User)
            .OrderByDescending(b => b.DateGranted)
            .ToListAsync();

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

