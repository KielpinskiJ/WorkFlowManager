using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public RequestsController(IRequestService requestService, UserManager<ApplicationUser> userManager)
    {
        _requestService = requestService;
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
    public IActionResult Create()
    {
        return View(new CreateRequestViewModel());
    }

    // POST: Requests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestViewModel model)
    {
        if (model.StartDate >= model.EndDate)
        {
            ModelState.AddModelError("EndDate", "End date must be after start date.");
        }

        if (model.StartDate < DateTime.Today)
        {
            ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _requestService.CreateRequestAsync(userId, model.Type, model.StartDate, model.EndDate);
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
    public async Task<IActionResult> Approve(int id, string? adminComment)
    {
        var result = await _requestService.ApproveRequestAsync(id, adminComment);
        if (!result)
        {
            TempData["Error"] = "Request not found.";
            return RedirectToAction(nameof(Manage));
        }

        TempData["Success"] = "Request approved successfully.";
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

