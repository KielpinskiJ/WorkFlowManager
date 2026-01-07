namespace WorkFlowManager.Models;

/// <summary>
/// Status of a leave request in the approval workflow.
/// </summary>
public enum RequestStatus
{
    /// <summary>
    /// Request is waiting for admin review.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Request has been approved by admin.
    /// </summary>
    Approved,
    
    /// <summary>
    /// Request has been rejected by admin.
    /// </summary>
    Rejected
}

