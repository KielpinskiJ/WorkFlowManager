using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Data;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;

namespace WorkFlowManager.Services;

/// <summary>
/// Service implementation for managing leave requests.
/// </summary>
public class RequestService : IRequestService
{
    private readonly ApplicationDbContext _context;

    public RequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequest> CreateRequestAsync(string userId, RequestType type, DateTime startDate, DateTime endDate, int? targetDepartmentId = null)
    {
        if (type == RequestType.Vacation && startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.");
        }

        await ValidateNoDuplicateRequestAsync(userId, type, startDate, endDate, targetDepartmentId);

        var request = new LeaveRequest
        {
            UserId = userId,
            Type = type,
            Status = RequestStatus.Pending,
            StartDate = startDate,
            EndDate = endDate,
            TargetDepartmentId = targetDepartmentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    /// <summary>
    /// Validates that no duplicate pending request exists for the user.
    /// </summary>
    private async Task ValidateNoDuplicateRequestAsync(string userId, RequestType type, DateTime startDate, DateTime endDate, int? targetDepartmentId)
    {
        var pendingRequestsQuery = _context.LeaveRequests
            .Where(r => r.UserId == userId && r.Status == RequestStatus.Pending && r.Type == type);

        bool duplicateExists;

        if (type == RequestType.Vacation)
        {
            duplicateExists = await pendingRequestsQuery
                .AnyAsync(r => r.StartDate == startDate && r.EndDate == endDate);
        }
        else
        {
            duplicateExists = await pendingRequestsQuery
                .AnyAsync(r => r.TargetDepartmentId == targetDepartmentId);
        }

        if (duplicateExists)
        {
            var message = type == RequestType.Vacation
                ? "You already have a pending vacation request for the same dates. Please wait for it to be processed."
                : "You already have a pending department change request for this department. Please wait for it to be processed.";

            throw new InvalidOperationException(message);
        }
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync()
    {
        return await _context.LeaveRequests
            .Include(r => r.User)
            .Include(r => r.TargetDepartment)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<LeaveRequest> Requests, int TotalCount)> GetPendingRequestsPagedAsync(RequestType? requestType, int page, int pageSize)
    {
        var query = _context.LeaveRequests
            .Include(r => r.User)
            .Include(r => r.TargetDepartment)
            .Where(r => r.Status == RequestStatus.Pending);

        if (requestType.HasValue)
        {
            query = query.Where(r => r.Type == requestType.Value);
        }

        var totalCount = await query.CountAsync();
        
        var requests = await query
            .OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (requests, totalCount);
    }

    public async Task<IEnumerable<LeaveRequest>> GetUserRequestsAsync(
        string userId, 
        int? months = null, 
        IEnumerable<RequestStatus>? excludeStatuses = null)
    {
        var query = _context.LeaveRequests
            .Where(r => r.UserId == userId);

        if (months.HasValue)
        {
            var startDate = DateTime.Today.AddMonths(-months.Value);
            query = query.Where(r => r.CreatedAt >= startDate);
        }

        if (excludeStatuses != null && excludeStatuses.Any())
        {
            var excludeList = excludeStatuses.ToList();
            query = query.Where(r => !excludeList.Contains(r.Status));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Include(r => r.User)
            .Include(r => r.TargetDepartment)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ApproveRequestAsync(int requestId, string? adminComment = null, bool autoTransfer = false)
    {
        var request = await _context.LeaveRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        
        if (request == null)
        {
            return false;
        }

        request.Status = RequestStatus.Approved;
        request.AdminComment = adminComment;

        // Auto-transfer user to target department if requested
        if (autoTransfer && request.Type == RequestType.DepartmentChange 
            && request.TargetDepartmentId.HasValue && request.User != null)
        {
            request.User.DepartmentId = request.TargetDepartmentId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectRequestAsync(int requestId, string? adminComment = null)
    {
        var request = await _context.LeaveRequests.FindAsync(requestId);
        
        if (request == null)
        {
            return false;
        }

        request.Status = RequestStatus.Rejected;
        request.AdminComment = adminComment;

        await _context.SaveChangesAsync();
        return true;
    }
}

