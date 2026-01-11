using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;
using WorkFlowManager.ViewModels;

namespace WorkFlowManager.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentService _departmentService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        IDepartmentService departmentService)
    {
        _userManager = userManager;
        _departmentService = departmentService;
    }

    /// <summary>
    /// Displays list of users with optional filtering by department and status.
    /// </summary>
    /// <param name="departmentId">Null shows all, 0 shows unassigned.</param>
    /// <param name="status">"active", "inactive", or null for all.</param>
    public async Task<IActionResult> Index(int? departmentId = null, string? status = null)
    {
        var query = _userManager.Users
            .Include(u => u.Department)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = departmentId.Value == 0
                ? query.Where(u => u.DepartmentId == null)
                : query.Where(u => u.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => u.IsActive);
            }
            else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => !u.IsActive);
            }
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var userViewModels = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserListViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}",
                DepartmentName = user.Department?.Name,
                IsActive = user.IsActive,
                Roles = roles
            });
        }

        var departments = await _departmentService.GetAllAsync();
        ViewBag.Departments = departments;
        ViewBag.SelectedDepartmentId = departmentId;
        ViewBag.SelectedStatus = status;
        ViewBag.TotalCount = userViewModels.Count;

        return View(userViewModels);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManager.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        var departments = await _departmentService.GetAllAsync();

        var viewModel = new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DepartmentId = user.DepartmentId,
            IsActive = user.IsActive,
            Departments = new SelectList(departments, "Id", "Name", user.DepartmentId)
        };

        return View(viewModel);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel viewModel)
    {
        if (id != viewModel.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            user.DepartmentId = viewModel.DepartmentId;
            user.IsActive = viewModel.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        var departments = await _departmentService.GetAllAsync();
        viewModel.Departments = new SelectList(departments, "Id", "Name", viewModel.DepartmentId);
        return View(viewModel);
    }
}
