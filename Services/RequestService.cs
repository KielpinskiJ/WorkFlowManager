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

    public async Task<LeaveRequest> CreateRequestAsync(string userId, RequestType type, DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.");
        }

        var request = new LeaveRequest
        {
            UserId = userId,
            Type = type,
            Status = RequestStatus.Pending,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync()
    {
        return await _context.LeaveRequests
            .Include(r => r.User)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetUserRequestsAsync(string userId)
    {
        return await _context.LeaveRequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ApproveRequestAsync(int requestId, string? adminComment = null)
    {
        var request = await _context.LeaveRequests.FindAsync(requestId);
        
        if (request == null)
        {
            return false;
        }

        request.Status = RequestStatus.Approved;
        request.AdminComment = adminComment;

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

