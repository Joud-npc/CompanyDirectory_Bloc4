using CompanyDirectory.Models;

namespace CompanyDirectory.Services
{
    public interface IPdfService
    {
        void GenerateEmployeePdf(Employee e, string outputPath);
    }
}