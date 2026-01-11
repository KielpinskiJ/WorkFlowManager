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

    private const int ShiftsPageSize = 32;

    /// <summary>
    /// Displays the current user's schedule with optional filtering.
    /// Available for all authenticated users.
    /// </summary>
    /// <param name="period">Time periods: currentWeek, previousWeek, currentMonth, previousMonth</param>
    /// <param name="hideCompleted"></param>
    /// <param name="hideToday"></param>
    /// <param name="hideUpcoming"></param>
    /// <param name="page"></param>
    public async Task<IActionResult> MySchedule(
        string period = "currentWeek",
        bool hideCompleted = false,
        bool hideToday = false,
        bool hideUpcoming = false,
        int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (page < 1) page = 1;

        // Calculate date range based on selected period
        var today = DateTime.Today;
        var now = DateTime.Now;
        DateTime periodStart, periodEnd;

        switch (period)
        {
            case "previousWeek":
                var daysUntilMondayPrev = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                var thisWeekStart = today.AddDays(-daysUntilMondayPrev);
                periodStart = thisWeekStart.AddDays(-7);
                periodEnd = thisWeekStart.AddMilliseconds(-1);
                break;
            case "currentMonth":
                periodStart = new DateTime(today.Year, today.Month, 1);
                periodEnd = periodStart.AddMonths(1).AddMilliseconds(-1);
                break;
            case "previousMonth":
                var firstOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
                periodStart = firstOfCurrentMonth.AddMonths(-1);
                periodEnd = firstOfCurrentMonth.AddMilliseconds(-1);
                break;
            case "currentWeek":
            default:
                var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                periodStart = today.AddDays(-daysUntilMonday);
                periodEnd = periodStart.AddDays(7).AddMilliseconds(-1);
                period = "currentWeek";
                break;
        }

        var allShifts = await _shiftService.GetShiftsForUserAsync(user.Id, periodStart, periodEnd);

        var filteredShifts = allShifts.Where(s =>
        {
            var isCompleted = s.EndTime < now;
            var isToday = s.StartTime.Date == today;
            var isUpcoming = s.StartTime.Date > today;

            if (hideCompleted && isCompleted && !isToday) return false;
            if (hideToday && isToday) return false;
            if (hideUpcoming && isUpcoming) return false;

            return true;
        }).ToList();

        // Calculate totals for entire period
        var totalCount = filteredShifts.Count;
        var totalHours = filteredShifts.Sum(s => s.DurationHours);
        var avgShiftLength = totalCount > 0 ? totalHours / totalCount : 0;

        var totalPages = (int)Math.Ceiling(totalCount / (double)ShiftsPageSize);
        if (page > totalPages && totalPages > 0) page = totalPages;

        var pagedShifts = filteredShifts
            .OrderBy(s => s.StartTime)
            .Skip((page - 1) * ShiftsPageSize)
            .Take(ShiftsPageSize)
            .ToList();

        ViewBag.PeriodStart = periodStart;
        ViewBag.PeriodEnd = periodEnd;
        ViewBag.UserName = $"{user.FirstName} {user.LastName}";
        ViewBag.SelectedPeriod = period;
        ViewBag.HideCompleted = hideCompleted;
        ViewBag.HideToday = hideToday;
        ViewBag.HideUpcoming = hideUpcoming;
        ViewBag.TotalHours = totalHours;
        ViewBag.AvgShiftLength = avgShiftLength;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        
        return View(pagedShifts);
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

