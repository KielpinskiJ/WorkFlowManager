namespace WorkFlowManager.Models;

/// <summary>
/// Types of leave requests that employees can submit.
/// </summary>
public enum RequestType
{
    /// <summary>
    /// Standard vacation/time-off request.
    /// </summary>
    Vacation,
    
    /// <summary>
    /// Request to change to a different department.
    /// </summary>
    DepartmentChange
}

