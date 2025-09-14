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
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"{e.FirstName} {e.LastName}").FontSize(20).Bold();
                        col.Item().Text($"Email: {e.Email}");
                        col.Item().Text($"Téléphone: {e.Phone} / {e.MobilePhone}");
                        col.Item().Text($"Site: {e.Site?.City ?? "N/A"}");
                        col.Item().Text($"Service: {e.Service?.Name ?? "N/A"}");
                    });
                });
            }).GeneratePdf(outputPath);
        }
    }
}