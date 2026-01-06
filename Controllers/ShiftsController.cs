using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Controllers;

/// <summary>
/// Controller for managing work shifts.
/// </summary>
[Authorize]
public class ShiftsController : Controller
{
    private readonly IShiftService _shiftService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ShiftsController(IShiftService shiftService, UserManager<ApplicationUser> userManager)
    {
        _shiftService = shiftService;
        _userManager = userManager;
    }

    /// <summary>
    /// Displays a list of all shifts sorted by date descending. Admin only.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var shifts = await _shiftService.GetAllShiftsAsync();
        return View(shifts);
    }

    /// <summary>
    /// Displays the form for creating a new shift. Admin only.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateShiftViewModel
        {
            Users = await GetUsersSelectListAsync()
        };
        return View(viewModel);
    }

    /// <summary>
    /// Handles the creation of a new shift. Admin only.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateShiftViewModel viewModel)
    {
        if (viewModel.EndTime <= viewModel.StartTime)
        {
            ModelState.AddModelError("EndTime", "End time must be after start time.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Users = await GetUsersSelectListAsync();
            return View(viewModel);
        }

        try
        {
            var shift = new WorkShift
            {
                UserId = viewModel.UserId,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime
            };

            await _shiftService.AddShiftAsync(shift);
            TempData["Success"] = "Shift created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            viewModel.Users = await GetUsersSelectListAsync();
            return View(viewModel);
        }
    }

    /// <summary>
    /// Displays shift details. Admin only.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var shift = await _shiftService.GetByIdAsync(id);
        if (shift == null)
        {
            return NotFound();
        }
        return View(shift);
    }

    /// <summary>
    /// Deletes a shift. Admin only.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _shiftService.DeleteAsync(id);
        if (result)
        {
            TempData["Success"] = "Shift deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete shift.";
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Displays the current user's schedule for the current week.
    /// Available for all authenticated users.
    /// </summary>
    public async Task<IActionResult> MySchedule()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Get current week boundaries (Monday to Sunday)
        var today = DateTime.Today;
        var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var weekStart = today.AddDays(-daysUntilMonday);
        var weekEnd = weekStart.AddDays(7).AddMilliseconds(-1); // End of Sunday

        var shifts = await _shiftService.GetShiftsForUserAsync(user.Id, weekStart, weekEnd);
        
        ViewBag.WeekStart = weekStart;
        ViewBag.WeekEnd = weekEnd;
        ViewBag.UserName = $"{user.FirstName} {user.LastName}";
        
        return View(shifts);
    }

    /// <summary>
    /// Gets a SelectList of all active users for dropdown.
    /// </summary>
    private async Task<SelectList> GetUsersSelectListAsync()
    {
        var users = _userManager.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new
            {
                u.Id,
                FullName = $"{u.FirstName} {u.LastName} ({u.Email})"
            })
            .ToList();

        return new SelectList(users, "Id", "FullName");
    }
}

