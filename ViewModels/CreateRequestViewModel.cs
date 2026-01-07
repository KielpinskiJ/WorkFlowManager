using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Target department for DepartmentChange requests.
    /// </summary>
    [Display(Name = "Target Department")]
    public int? TargetDepartmentId { get; set; }

    // For dropdown population
    public IEnumerable<SelectListItem>? Departments { get; set; }
}

