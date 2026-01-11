using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WorkFlowManager.Services.Interfaces;

namespace WorkFlowManager.Controllers;

/// <summary>
/// Controller for displaying statistics and charts. Admin only.
/// </summary>
[Authorize(Roles = "Admin")]
public class StatsController : Controller
{
    private readonly IShiftService _shiftService;

    public StatsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    /// <summary>
    /// Displays the statistics dashboard with Chart.js visualization.
    /// </summary>
    /// <param name="months">Defaults to 3.</param>
    public async Task<IActionResult> Index(int months = 3)
    {
        var validMonths = new[] { 1, 3, 6, 12 };
        if (!validMonths.Contains(months))
        {
            months = 3;
        }

        var stats = await _shiftService.GetMonthlyStatsAsync(months);
        
        // Serialize data for Chart.js
        var labels = stats.Select(s => s.DepartmentName).ToList();
        var hours = stats.Select(s => Math.Round(s.TotalHours, 1)).ToList();
        var counts = stats.Select(s => s.ShiftCount).ToList();
        
        ViewBag.ChartLabels = JsonSerializer.Serialize(labels);
        ViewBag.ChartHours = JsonSerializer.Serialize(hours);
        ViewBag.ChartCounts = JsonSerializer.Serialize(counts);
        ViewBag.SelectedMonths = months;
        
        return View(stats);
    }
}

