using WorkFlowManager.Models;

namespace WorkFlowManager.Services.Interfaces;

/// <summary>
/// Service interface for managing leave requests.
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Creates a new leave request with Pending status.
    /// </summary>
    /// <param name="userId">The ID of the user submitting the request.</param>
    /// <param name="type">The type of request (Vacation or DepartmentChange).</param>
    /// <param name="startDate">Start date of the leave.</param>
    /// <param name="endDate">End date of the leave.</param>
    /// <param name="targetDepartmentId">Target department for department change requests.</param>
    /// <returns>The created leave request.</returns>
    Task<LeaveRequest> CreateRequestAsync(string userId, RequestType type, DateTime startDate, DateTime endDate, int? targetDepartmentId = null);

    /// <summary>
    /// Gets all pending requests with user details included.
    /// </summary>
    /// <returns>List of pending leave requests.</returns>
    Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync();

    /// <summary>
    /// Gets all requests for a specific user with optional filtering.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="months">Number of months to include (null for all time).</param>
    /// <param name="excludeStatuses">Statuses to exclude from results.</param>
    /// <returns>List of user's leave requests.</returns>
    Task<IEnumerable<LeaveRequest>> GetUserRequestsAsync(
        string userId, 
        int? months = null, 
        IEnumerable<RequestStatus>? excludeStatuses = null);

    /// <summary>
    /// Gets a single request by ID with user details.
    /// </summary>
    /// <param name="id">The request ID.</param>
    /// <returns>The leave request or null if not found.</returns>
    Task<LeaveRequest?> GetByIdAsync(int id);

    /// <summary>
    /// Approves a leave request.
    /// </summary>
    /// <param name="requestId">The request ID to approve.</param>
    /// <param name="adminComment">Optional comment from admin.</param>
    /// <param name="autoTransfer">For DepartmentChange requests: automatically transfer user to target department.</param>
    /// <returns>True if approved successfully, false if request not found.</returns>
    Task<bool> ApproveRequestAsync(int requestId, string? adminComment = null, bool autoTransfer = false);

    /// <summary>
    /// Rejects a leave request.
    /// </summary>
    /// <param name="requestId">The request ID to reject.</param>
    /// <param name="adminComment">Optional comment explaining rejection.</param>
    /// <returns>True if rejected successfully, false if request not found.</returns>
    Task<bool> RejectRequestAsync(int requestId, string? adminComment = null);
}

