using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _db;
        public EmployeeService(ApplicationDbContext db) { _db = db; }

        public async Task<List<Employee>> GetAllAsync() =>
            await _db.Employees.Include(e => e.Site).Include(e => e.Service).ToListAsync();

        public async Task<Employee?> GetByIdAsync(int id) =>
            await _db.Employees.Include(e => e.Site).Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task AddAsync(Employee e)
        {
            _db.Employees.Add(e);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee e)
        {
            _db.Employees.Update(e);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Employees.FindAsync(id);
            if (e != null) { _db.Employees.Remove(e); await _db.SaveChangesAsync(); }
        }

        public async Task<List<Site>> GetSitesAsync() =>
            await _db.Sites.ToListAsync();

        public async Task<List<Service>> GetServicesAsync() =>
            await _db.Services.ToListAsync();
    }
}