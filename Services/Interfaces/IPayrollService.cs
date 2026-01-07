using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Services.Interfaces;

/// <summary>
/// Service interface for generating payroll reports.
/// </summary>
public interface IPayrollService
{
    /// <summary>
    /// Generates a payroll report for the specified month and year.
    /// Calculates: (TotalHours * HourlyRate) + Bonuses for each employee.
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year</param>
    /// <returns>Complete payroll report with all employee entries.</returns>
    Task<PayrollReportViewModel> GeneratePayrollAsync(int month, int year);

    /// <summary>
    /// Gets payroll entry for a specific user for the specified month and year.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year</param>
    /// <returns>Payroll entry for the user, or null if not found.</returns>
    Task<PayrollEntryViewModel?> GetUserPayrollAsync(string userId, int month, int year);
}

