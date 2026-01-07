using Microsoft.AspNetCore.Identity;

namespace WorkFlowManager.Models;

/// <summary>
/// Extended Identity user with additional employee properties.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Department relation
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    // Work shifts relation
    public ICollection<WorkShift> Shifts { get; set; } = new List<WorkShift>();
    
    // Leave requests relation
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    
    // Bonuses relation
    public ICollection<Bonus> Bonuses { get; set; } = new List<Bonus>();
}
