using CompanyDirectory.Models;

namespace CompanyDirectory.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee e);
        Task UpdateAsync(Employee e);
        Task DeleteAsync(int id);
        Task<List<Site>> GetSitesAsync();
        Task<List<Service>> GetServicesAsync();
    }
}