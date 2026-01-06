using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Controllers;

/// <summary>
/// Controller for managing work shifts. Admin-only access.
/// </summary>
[Authorize(Roles = "Admin")]
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
    /// Displays a list of all shifts sorted by date descending.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var shifts = await _shiftService.GetAllShiftsAsync();
        return View(shifts);
    }

    /// <summary>
    /// Displays the form for creating a new shift.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateShiftViewModel
        {
            Users = await GetUsersSelectListAsync()
        };
        return View(viewModel);
    }

    /// <summary>
    /// Handles the creation of a new shift.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
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
    /// Displays shift details.
    /// </summary>
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
    /// Deletes a shift.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
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

