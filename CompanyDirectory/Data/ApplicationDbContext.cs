using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Site> Sites => Set<Site>();
        public DbSet<Service> Services => Set<Service>();
    }
}