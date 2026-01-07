using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;

namespace WorkFlowManager.Controllers;

/// <summary>
/// Controller for generating and viewing payroll reports.
/// </summary>
[Authorize]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PayrollController(IPayrollService payrollService, UserManager<ApplicationUser> userManager)
    {
        _payrollService = payrollService;
        _userManager = userManager;
    }

    // GET: Payroll - Admin only
    // GET: Payroll?month=1&year=2026
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int? month, int? year)
    {
        // Default to current month/year if not specified
        var targetMonth = month ?? DateTime.Today.Month;
        var targetYear = year ?? DateTime.Today.Year;

        // Validate month range
        if (targetMonth < 1 || targetMonth > 12)
        {
            targetMonth = DateTime.Today.Month;
        }

        // Validate year range (reasonable bounds)
        if (targetYear < 2020 || targetYear > DateTime.Today.Year + 1)
        {
            targetYear = DateTime.Today.Year;
        }

        var report = await _payrollService.GeneratePayrollAsync(targetMonth, targetYear);

        // Pass available months/years for the selector
        ViewBag.AvailableYears = Enumerable.Range(2024, DateTime.Today.Year - 2024 + 2).ToList();
        
        return View(report);
    }

    // GET: Payroll/MyPayroll - Employee view of their own payroll
    public async Task<IActionResult> MyPayroll(int? month, int? year)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var targetMonth = month ?? DateTime.Today.Month;
        var targetYear = year ?? DateTime.Today.Year;

        if (targetMonth < 1 || targetMonth > 12)
        {
            targetMonth = DateTime.Today.Month;
        }

        if (targetYear < 2020 || targetYear > DateTime.Today.Year + 1)
        {
            targetYear = DateTime.Today.Year;
        }

        var payrollEntry = await _payrollService.GetUserPayrollAsync(userId, targetMonth, targetYear);

        ViewBag.Month = targetMonth;
        ViewBag.Year = targetYear;
        ViewBag.MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(targetMonth);
        ViewBag.AvailableYears = Enumerable.Range(2024, DateTime.Today.Year - 2024 + 2).ToList();

        return View(payrollEntry);
    }
}

