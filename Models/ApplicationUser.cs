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
}
