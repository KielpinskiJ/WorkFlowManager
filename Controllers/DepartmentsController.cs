using Microsoft.AspNetCore.Mvc;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;

namespace WorkFlowManager.Controllers;

public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: Departments
    public async Task<IActionResult> Index()
    {
        var departments = await _departmentService.GetAllAsync();
        return View(departments);
    }

    // GET: Departments/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    // GET: Departments/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    // GET: Departments/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _departmentService.DeleteAsync(id);
        if (!result)
            return NotFound();

        TempData["Success"] = "Department deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
