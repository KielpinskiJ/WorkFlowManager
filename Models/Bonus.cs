using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Models;

/// <summary>
/// Represents a bonus payment granted to an employee.
/// </summary>
public class Bonus
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateGranted { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 500 characters")]
    public string Reason { get; set; } = string.Empty;

    // Navigation property
    public ApplicationUser? User { get; set; }
}

