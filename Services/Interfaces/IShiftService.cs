using WorkFlowManager.Models;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Services.Interfaces;

/// <summary>
/// Service interface for managing work shifts.
/// </summary>
public interface IShiftService
{
    /// <summary>
    /// Adds a new shift with validation (EndTime must be greater than StartTime).
    /// </summary>
    /// <param name="shift">The shift to add.</param>
    /// <returns>The created shift.</returns>
    /// <exception cref="ArgumentException">Thrown when EndTime is not greater than StartTime.</exception>
    Task<WorkShift> AddShiftAsync(WorkShift shift);
    
    /// <summary>
    /// Gets all shifts for a specific user within a date range.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="start">Start of the date range.</param>
    /// <param name="end">End of the date range.</param>
    /// <returns>Collection of shifts within the specified range.</returns>
    Task<IEnumerable<WorkShift>> GetShiftsForUserAsync(string userId, DateTime start, DateTime end);
    
    /// <summary>
    /// Gets all shifts including user data.
    /// </summary>
    /// <returns>Collection of all shifts with user information.</returns>
    Task<IEnumerable<WorkShift>> GetAllShiftsAsync();
    
    /// <summary>
    /// Gets a shift by its identifier.
    /// </summary>
    /// <param name="id">The shift identifier.</param>
    /// <returns>The shift if found, null otherwise.</returns>
    Task<WorkShift?> GetByIdAsync(int id);
    
    /// <summary>
    /// Deletes a shift by its identifier.
    /// </summary>
    /// <param name="id">The shift identifier.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Gets statistics grouped by department for a specified number of months.
    /// </summary>
    /// <param name="months">Number of months to include (1, 3, 6, or 12). Defaults to 3.</param>
    /// <returns>Collection of department statistics with total hours worked.</returns>
    Task<IEnumerable<DepartmentStatsDto>> GetMonthlyStatsAsync(int months = 3);
    
    /// <summary>
    /// Gets paginated shifts for a specific date or all shifts if date is null.
    /// </summary>
    /// <param name="date">Optional date to filter shifts. Null returns all shifts.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Tuple with shifts collection and total count.</returns>
    Task<(IEnumerable<WorkShift> Shifts, int TotalCount)> GetShiftsByDatePagedAsync(DateTime? date, int page, int pageSize);
    
    /// <summary>
    /// Gets the total count of all employees in the system (active and inactive).
    /// </summary>
    /// <returns>Total number of employees.</returns>
    Task<int> GetTotalEmployeeCountAsync();
}

