using WorkFlowManager.Models;

namespace WorkFlowManager.Services.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task<Department?> UpdateAsync(Department department);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
