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

    // GET: Requests - Show current user's requests
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var requests = await _requestService.GetUserRequestsAsync(userId);
        return View(requests);
    }

    // GET: Requests/Create
    public async Task<IActionResult> Create()
    {
        var departments = await _departmentService.GetAllAsync();
        var model = new CreateRequestViewModel
        {
            Departments = departments.Select(d => new SelectListItem
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
            // Repopulate departments dropdown
            var departments = await _departmentService.GetAllAsync();
            model.Departments = departments.Select(d => new SelectListItem
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

        await _requestService.CreateRequestAsync(
            userId, 
            model.Type, 
            model.StartDate ?? DateTime.Today, 
            model.EndDate ?? DateTime.Today,
            model.TargetDepartmentId);
            
        TempData["Success"] = "Request submitted successfully. Awaiting admin approval.";
        return RedirectToAction(nameof(Index));
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

    // GET: Requests/Manage - Admin only, show pending requests
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage()
    {
        var pendingRequests = await _requestService.GetPendingRequestsAsync();
        return View(pendingRequests);
    }

    // POST: Requests/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, string? adminComment, bool autoTransfer = false)
    {
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

