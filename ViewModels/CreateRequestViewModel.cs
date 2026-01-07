using System.ComponentModel.DataAnnotations;
using WorkFlowManager.Models;

namespace WorkFlowManager.ViewModels;

/// <summary>
/// ViewModel for creating a new leave request.
/// </summary>
public class CreateRequestViewModel
{
    [Required(ErrorMessage = "Request type is required")]
    [Display(Name = "Request Type")]
    public RequestType Type { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "End date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(2);
}

