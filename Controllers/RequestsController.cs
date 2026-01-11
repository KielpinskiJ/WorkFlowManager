using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Controllers;

/// <summary>
/// Controller for managing leave requests.
/// </summary>
[Authorize]
public class RequestsController : Controller
{
    private const int MaxAdminCommentLength = 500;

    private readonly IRequestService _requestService;
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RequestsController(
        IRequestService requestService, 
        IDepartmentService departmentService,
        UserManager<ApplicationUser> userManager)
    {
        _requestService = requestService;
        _departmentService = departmentService;
        _userManager = userManager;
    }

    private const int RequestsPageSize = 20;

    // GET: Requests - Show current user's requests with optional filtering
    public async Task<IActionResult> Index(
        int? months = null, 
        bool hidePending = false, 
        bool hideApproved = false, 
        bool hideRejected = false,
        int page = 1)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var validMonths = new int?[] { null, 1, 3, 6, 12 };
        if (!validMonths.Contains(months))
        {
            months = null;
        }

        if (page < 1) page = 1;

        var excludeStatuses = new List<RequestStatus>();
        if (hidePending) excludeStatuses.Add(RequestStatus.Pending);
        if (hideApproved) excludeStatuses.Add(RequestStatus.Approved);
        if (hideRejected) excludeStatuses.Add(RequestStatus.Rejected);

        var allRequests = await _requestService.GetUserRequestsAsync(
            userId, 
            months, 
            excludeStatuses.Any() ? excludeStatuses : null);

        // Pagination
        var totalCount = allRequests.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)RequestsPageSize);
        if (page > totalPages && totalPages > 0) page = totalPages;

        var pagedRequests = allRequests
            .Skip((page - 1) * RequestsPageSize)
            .Take(RequestsPageSize)
            .ToList();

        ViewBag.SelectedMonths = months;
        ViewBag.HidePending = hidePending;
        ViewBag.HideApproved = hideApproved;
        ViewBag.HideRejected = hideRejected;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return View(pagedRequests);
    }

    // GET: Requests/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var departments = await _departmentService.GetAllAsync();
        var model = new CreateRequestViewModel
        {
            Departments = departments
            .Where(d => d.Id != user?.DepartmentId)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            })
        };
        return View(model);
    }

    // POST: Requests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestViewModel model)
    {
        // Validation depends on request type
        if (model.Type == RequestType.Vacation)
        {
            if (!model.StartDate.HasValue)
            {
                ModelState.AddModelError("StartDate", "Start date is required for vacation requests.");
            }
            if (!model.EndDate.HasValue)
            {
                ModelState.AddModelError("EndDate", "End date is required for vacation requests.");
            }
            if (model.StartDate.HasValue && model.EndDate.HasValue)
            {
                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                }
                if (model.StartDate < DateTime.Today)
                {
                    ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
                }
            }
        }
        else if (model.Type == RequestType.DepartmentChange)
        {
            if (!model.TargetDepartmentId.HasValue)
            {
                ModelState.AddModelError("TargetDepartmentId", "Please select a target department.");
            }
        }

        if (!ModelState.IsValid)
        {
            // Repopulate departments dropdown (exclude user's current department)
            var currentUser = await _userManager.GetUserAsync(User);
            var departments = await _departmentService.GetAllAsync();
            model.Departments = departments
                .Where(d => d.Id != currentUser?.DepartmentId)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                });
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _requestService.CreateRequestAsync(
                userId, 
                model.Type, 
                model.StartDate ?? DateTime.Today, 
                model.EndDate ?? DateTime.Today,
                model.TargetDepartmentId);
                
            TempData["Success"] = "Request submitted successfully. Awaiting admin approval.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            
            // Repopulate departments dropdown
            var currentUser = await _userManager.GetUserAsync(User);
            var departments = await _departmentService.GetAllAsync();
            model.Departments = departments
                .Where(d => d.Id != currentUser?.DepartmentId)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                });
            return View(model);
        }
    }

    // GET: Requests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var request = await _requestService.GetByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        // Users can only see their own requests, admins can see all
        var userId = _userManager.GetUserId(User);
        if (request.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(request);
    }

    private const int ManageRequestsPageSize = 20;

    /// <summary>
    /// Displays paginated pending requests with optional type filtering. Admin only.
    /// </summary>
    /// <param name="requestType">"Vacation" or "DepartmentChange". Null shows all.</param>
    /// <param name="page"></param>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage(string? requestType = null, int page = 1)
    {
        if (page < 1) page = 1;

        RequestType? filterType = null;
        if (!string.IsNullOrEmpty(requestType) && Enum.TryParse<RequestType>(requestType, out var parsedType))
        {
            filterType = parsedType;
        }

        var (requests, totalCount) = await _requestService.GetPendingRequestsPagedAsync(
            filterType, page, ManageRequestsPageSize);

        var totalPages = (int)Math.Ceiling(totalCount / (double)ManageRequestsPageSize);
        if (page > totalPages && totalPages > 0) page = totalPages;

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        ViewBag.SelectedType = requestType;

        return View(requests);
    }

    // POST: Requests/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, string? adminComment, bool autoTransfer = false)
    {
        if (!string.IsNullOrEmpty(adminComment) && adminComment.Length > MaxAdminCommentLength)
        {
            TempData["Error"] = $"Comment cannot exceed {MaxAdminCommentLength} characters.";
            return RedirectToAction(nameof(Manage));
        }

        var result = await _requestService.ApproveRequestAsync(id, adminComment, autoTransfer);
        if (!result)
        {
            TempData["Error"] = "Request not found.";
            return RedirectToAction(nameof(Manage));
        }

        var message = autoTransfer 
            ? "Request approved and employee transferred to new department." 
            : "Request approved successfully.";
        TempData["Success"] = message;
        return RedirectToAction(nameof(Manage));
    }

    // POST: Requests/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id, string? adminComment)
    {
        if (!string.IsNullOrEmpty(adminComment) && adminComment.Length > MaxAdminCommentLength)
        {
            TempData["Error"] = $"Comment cannot exceed {MaxAdminCommentLength} characters.";
            return RedirectToAction(nameof(Manage));
        }

        var result = await _requestService.RejectRequestAsync(id, adminComment);
        if (!result)
        {
            TempData["Error"] = "Request not found.";
            return RedirectToAction(nameof(Manage));
        }

        TempData["Success"] = "Request rejected.";
        return RedirectToAction(nameof(Manage));
    }
}

