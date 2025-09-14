using CompanyDirectory.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CompanyDirectory.Services
{
    public class PdfService : IPdfService
    {
        public void GenerateEmployeePdf(Employee e, string outputPath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);
                    page.Size(PageSizes.A4);
                    page.Header().Text($"{e.FirstName} {e.LastName}").FontSize(18).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Email: {e.Email}");
                        col.Item().Text($"Téléphone: {e.Phone}");
                        col.Item().Text($"Mobile: {e.Phone}");
                        col.Item().Text($"Site: {e.Site?.Ville ?? "N/A"}");
                        col.Item().Text($"Service: {e.Service?.Nom ?? "N/A"}");
                    });
                });
            }).GeneratePdf(outputPath);
        }
    }
}