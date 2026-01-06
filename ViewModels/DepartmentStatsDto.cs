namespace WorkFlowManager.ViewModels;

/// <summary>
/// Data Transfer Object for department statistics.
/// Used for displaying aggregated work hours per department.
/// </summary>
public class DepartmentStatsDto
{
    /// <summary>
    /// Name of the department.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Total hours worked in this department.
    /// </summary>
    public double TotalHours { get; set; }
    
    /// <summary>
    /// Number of shifts in this department.
    /// </summary>
    public int ShiftCount { get; set; }
}

