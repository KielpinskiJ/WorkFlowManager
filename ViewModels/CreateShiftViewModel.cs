using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WorkFlowManager.ViewModels;

/// <summary>
/// ViewModel for creating a new work shift.
/// </summary>
public class CreateShiftViewModel
{
    [Required(ErrorMessage = "Please select an employee.")]
    [Display(Name = "Employee")]
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Start time is required.")]
    [Display(Name = "Start Time")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.Today.AddHours(9);
    
    [Required(ErrorMessage = "End time is required.")]
    [Display(Name = "End Time")]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; } = DateTime.Today.AddHours(17);
    
    /// <summary>
    /// List of available users for dropdown selection.
    /// </summary>
    public SelectList? Users { get; set; }
}

