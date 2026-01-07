namespace WorkFlowManager.ViewModels;

/// <summary>
/// ViewModel for displaying payroll data for a single employee.
/// </summary>
public class PayrollEntryViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = "Unassigned";
    public decimal HourlyRate { get; set; }
    public double TotalHours { get; set; }
    public decimal BasePayment { get; set; }
    public decimal BonusTotal { get; set; }
    public decimal FinalSalary { get; set; }
}

/// <summary>
/// ViewModel for the entire payroll report.
/// </summary>
public class PayrollReportViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public List<PayrollEntryViewModel> Entries { get; set; } = new();
    
    // Summary
    public decimal TotalPayroll => Entries.Sum(e => e.FinalSalary);
    public decimal TotalBonuses => Entries.Sum(e => e.BonusTotal);
    public double TotalHours => Entries.Sum(e => e.TotalHours);
    public int EmployeeCount => Entries.Count;
}

