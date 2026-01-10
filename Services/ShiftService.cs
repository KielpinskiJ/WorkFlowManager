using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Data;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Services;

/// <summary>
/// Service for managing work shifts with business logic and validation.
/// </summary>
public class ShiftService : IShiftService
{
    private readonly ApplicationDbContext _context;

    public ShiftService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkShift> AddShiftAsync(WorkShift shift)
    {
        ValidateShiftTimes(shift);
        
        // Snapshot the hourly rate at shift creation time for accurate payroll
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == shift.UserId);
        
        shift.HourlyRateSnapshot = user?.Department?.HourlyRate ?? 0;
        
        _context.WorkShifts.Add(shift);
        await _context.SaveChangesAsync();
        return shift;
    }

    public async Task<IEnumerable<WorkShift>> GetShiftsForUserAsync(string userId, DateTime start, DateTime end)
    {
        return await _context.WorkShifts
            .Include(s => s.User)
            .Where(s => s.UserId == userId && s.StartTime >= start && s.EndTime <= end)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkShift>> GetAllShiftsAsync()
    {
        return await _context.WorkShifts
            .Include(s => s.User)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<WorkShift?> GetByIdAsync(int id)
    {
        return await _context.WorkShifts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var shift = await _context.WorkShifts.FindAsync(id);
        if (shift == null)
            return false;
        
        _context.WorkShifts.Remove(shift);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets work statistics grouped by department for the last 3 months.
    /// </summary>
    /// <returns>Collection of department statistics ordered by total hours descending.</returns>
    public async Task<IEnumerable<DepartmentStatsDto>> GetMonthlyStatsAsync()
    {
        var threeMonthsAgo = DateTime.Now.AddMonths(-3);
        
        // Fetch data to memory first, then perform grouping with calculations
        var shifts = await _context.WorkShifts
            .Include(s => s.User)
                .ThenInclude(u => u!.Department)
            .Where(s => s.StartTime >= threeMonthsAgo)
            .Where(s => s.User != null && s.User.Department != null)
            .ToListAsync();
        
        // Group and calculate on client side
        var stats = shifts
            .GroupBy(s => s.User!.Department!.Name)
            .Select(group => new DepartmentStatsDto
            {
                DepartmentName = group.Key,
                TotalHours = group.Sum(s => (s.EndTime - s.StartTime).TotalHours),
                ShiftCount = group.Count()
            })
            .OrderByDescending(d => d.TotalHours)
            .ToList();
        
        return stats;
    }

    /// <summary>
    /// Validates that EndTime is greater than StartTime.
    /// </summary>
    /// <param name="shift">The shift to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateShiftTimes(WorkShift shift)
    {
        if (shift.EndTime <= shift.StartTime)
        {
            throw new ArgumentException("End time must be greater than start time.", nameof(shift));
        }
    }
}

