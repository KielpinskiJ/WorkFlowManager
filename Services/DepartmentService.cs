using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Data;
using WorkFlowManager.Models;
using WorkFlowManager.Services.Interfaces;

namespace WorkFlowManager.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;

    public DepartmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department?> UpdateAsync(Department department)
    {
        var existing = await _context.Departments.FindAsync(department.Id);
        if (existing == null)
            return null;

        existing.Name = department.Name;
        existing.Description = department.Description;
        existing.HourlyRate = department.HourlyRate;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return false;

        if (department.Users != null && department.Users.Any())
        {
            // Prevent deletion if there are users assigned to this department
            return false;
        }
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Departments.AnyAsync(d => d.Id == id);
    }
}
