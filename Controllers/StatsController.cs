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
    public async Task<IActionResult> Index()
    {
        var stats = await _shiftService.GetMonthlyStatsAsync();
        
        // Serialize data for Chart.js
        var labels = stats.Select(s => s.DepartmentName).ToList();
        var hours = stats.Select(s => Math.Round(s.TotalHours, 1)).ToList();
        var counts = stats.Select(s => s.ShiftCount).ToList();
        
        ViewBag.ChartLabels = JsonSerializer.Serialize(labels);
        ViewBag.ChartHours = JsonSerializer.Serialize(hours);
        ViewBag.ChartCounts = JsonSerializer.Serialize(counts);
        
        return View(stats);
    }
}

