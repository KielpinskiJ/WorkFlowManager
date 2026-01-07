using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Models;

/// <summary>
/// Represents an employee's leave or department change request.
/// </summary>
public class LeaveRequest
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public RequestType Type { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [StringLength(500)]
    public string? AdminComment { get; set; }

    /// <summary>
    /// Date when the request was submitted.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser? User { get; set; }
}

