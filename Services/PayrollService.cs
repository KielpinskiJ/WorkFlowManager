using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Data;
using WorkFlowManager.Services.Interfaces;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Services;

/// <summary>
/// Service for generating payroll reports.
/// Combines data from Departments (rates), WorkShifts (hours), and Bonuses.
/// </summary>
public class PayrollService : IPayrollService
{
    private readonly ApplicationDbContext _context;

    public PayrollService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollReportViewModel> GeneratePayrollAsync(int month, int year)
    {
        // Define the date range for the month
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Get all active users with their departments, shifts, and bonuses
        var users = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Shifts)
            .Include(u => u.Bonuses)
            .Where(u => u.IsActive)
            .ToListAsync();

        var entries = new List<PayrollEntryViewModel>();

        foreach (var user in users)
        {
            // Calculate total hours from shifts in the specified month
            var monthlyShifts = user.Shifts
                .Where(s => s.StartTime >= startDate && s.StartTime < endDate)
                .ToList();

            var totalHours = monthlyShifts
                .Sum(s => (s.EndTime - s.StartTime).TotalHours);

            // Get hourly rate from department (default to 0 if no department)
            var hourlyRate = user.Department?.HourlyRate ?? 0;

            // Calculate base payment
            var basePayment = (decimal)totalHours * hourlyRate;

            // Sum bonuses granted in the specified month
            var bonusTotal = user.Bonuses
                .Where(b => b.DateGranted >= startDate && b.DateGranted < endDate)
                .Sum(b => b.Amount);

            // Calculate final salary
            var finalSalary = basePayment + bonusTotal;

            entries.Add(new PayrollEntryViewModel
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                DepartmentName = user.Department?.Name ?? "Unassigned",
                HourlyRate = hourlyRate,
                TotalHours = Math.Round(totalHours, 2),
                BasePayment = Math.Round(basePayment, 2),
                BonusTotal = bonusTotal,
                FinalSalary = Math.Round(finalSalary, 2)
            });
        }

        // Sort by department, then by name
        entries = entries
            .OrderBy(e => e.DepartmentName)
            .ThenBy(e => e.FullName)
            .ToList();

        return new PayrollReportViewModel
        {
            Month = month,
            Year = year,
            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
            GeneratedAt = DateTime.Now,
            Entries = entries
        };
    }

    public async Task<PayrollEntryViewModel?> GetUserPayrollAsync(string userId, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Shifts)
            .Include(u => u.Bonuses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        var monthlyShifts = user.Shifts
            .Where(s => s.StartTime >= startDate && s.StartTime < endDate)
            .ToList();

        var totalHours = monthlyShifts.Sum(s => (s.EndTime - s.StartTime).TotalHours);
        var hourlyRate = user.Department?.HourlyRate ?? 0;
        var basePayment = (decimal)totalHours * hourlyRate;
        var bonusTotal = user.Bonuses
            .Where(b => b.DateGranted >= startDate && b.DateGranted < endDate)
            .Sum(b => b.Amount);

        return new PayrollEntryViewModel
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email ?? string.Empty,
            DepartmentName = user.Department?.Name ?? "Unassigned",
            HourlyRate = hourlyRate,
            TotalHours = Math.Round(totalHours, 2),
            BasePayment = Math.Round(basePayment, 2),
            BonusTotal = bonusTotal,
            FinalSalary = Math.Round(basePayment + bonusTotal, 2)
        };
    }
}

