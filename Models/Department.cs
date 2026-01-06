using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Models;

public class Department
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Department name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 10000, ErrorMessage = "Hourly rate must be between 0.01 and 10000")]
    public decimal HourlyRate { get; set; }

    // Navigation property
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
