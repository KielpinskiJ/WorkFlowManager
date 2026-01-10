using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;

namespace WorkFlowManager.Controllers;

[Authorize]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentsController(
        IDepartmentService departmentService,
        UserManager<ApplicationUser> userManager)
    {
        _departmentService = departmentService;
        _userManager = userManager;
    }

    // GET: Departments - Available for all logged-in users
    public async Task<IActionResult> Index()
    {
        var departments = await _departmentService.GetAllAsync();
        
        // Get current user's department for highlighting
        var user = await _userManager.GetUserAsync(User);
        ViewBag.UserDepartmentId = user?.DepartmentId;
        ViewBag.IsAdmin = User.IsInRole("Admin");
        
        return View(departments);
    }

    // GET: Departments/Details/5 - Admin only
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    // GET: Departments/Create - Admin only
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Departments/Create - Admin only
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Department department)
    {
        if (ModelState.IsValid)
        {
            await _departmentService.CreateAsync(department);
            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Edit/5 - Admin only
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    // POST: Departments/Edit/5 - Admin only
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _departmentService.UpdateAsync(department);
            if (result == null)
                return NotFound();

            TempData["Success"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Delete/5 - Admin only
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    // POST: Departments/Delete/5 - Admin only
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        if (department.Users != null && department.Users.Any())
        {
            TempData["Error"] = "Cannot delete department with assigned employees. Please reassign employees first.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _departmentService.DeleteAsync(id);
        if (!result)
        {
            TempData["Error"] = "Failed to delete department.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Department deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
