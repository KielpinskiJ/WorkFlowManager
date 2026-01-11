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

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000 PLN")]
    [DataType(DataType.Currency)]
    [Display(Name = "Bonus Amount (PLN)")]
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

