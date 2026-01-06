using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WorkFlowManager.ViewModels;

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    public SelectList? Departments { get; set; }
}
