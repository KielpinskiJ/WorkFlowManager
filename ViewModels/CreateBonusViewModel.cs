using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WorkFlowManager.ViewModels;

/// <summary>
/// ViewModel for creating a new bonus.
/// </summary>
public class CreateBonusViewModel
{
    [Required(ErrorMessage = "Please select an employee")]
    [Display(Name = "Employee")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
    [DataType(DataType.Currency)]
    [Display(Name = "Bonus Amount (PLN)")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 500 characters")]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date Granted")]
    public DateTime DateGranted { get; set; } = DateTime.Today;

    // For dropdown population
    public IEnumerable<SelectListItem>? Users { get; set; }
}

