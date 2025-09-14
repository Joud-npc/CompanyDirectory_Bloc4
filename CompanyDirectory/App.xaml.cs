using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows; // ✅ Nécessaire pour Application et StartupEventArgs
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CompanyDirectory
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var loginWindow = new LoginWindow(); // ou MainWindow si tu démarres direct
            loginWindow.Show(); // très important

            // --- CONFIGURATION ---
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            using var context = new ApplicationDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            if (!context.Sites.Any())
            {
                SeedDatabaseAsync(context).Wait();
            }
        }

        private async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            // Vérifier si MobilePhone existe dans Employee
            bool hasMobile = typeof(Employee).GetProperty("MobilePhone") != null;

            // Sites
            var sites = new List<Site>
            {
                new() { Ville = "Paris" },
                new() { Ville = "Lyon" },
                new() { Ville = "Marseille" },
                new() { Ville = "Toulouse" },
                new() { Ville = "Bordeaux" }
            };
            context.Sites.AddRange(sites);

            // Services
            var services = new List<Service>
            {
                new() { Nom = "Comptabilité" },
                new() { Nom = "Production" },
                new() { Nom = "Accueil" },
                new() { Nom = "R&D" },
                new() { Nom = "Marketing" }
            };
            context.Services.AddRange(services);

            await context.SaveChangesAsync();

            // Appel API RandomUser
            using var http = new HttpClient();
            int batchSize = 100;
            var rand = new Random();

            for (int i = 0; i < 1000; i += batchSize)
            {
                int fetch = Math.Min(batchSize, 1000 - i);
                var response = await http.GetStringAsync($"https://randomuser.me/api/?results={fetch}&nat=fr");
                var json = JObject.Parse(response);
                var results = json["results"] as JArray;
                if (results == null) continue;

                foreach (var r in results)
                {
                    var first = (string?)r["name"]?["first"] ?? "X";
                    var last = (string?)r["name"]?["last"] ?? "X";
                    var email = (string?)r["email"] ?? $"{first}.{last}@example.com";
                    var phone = (string?)r["phone"] ?? "";
                    var cell = (string?)r["cell"] ?? "";

                    var site = sites[rand.Next(sites.Count)];
                    var service = services[rand.Next(services.Count)];

                    var employee = new Employee
                    {
                        FirstName = first,
                        LastName = last,
                        Email = email,
                        Phone = phone,
                        SiteId = site.Id,
                        ServiceId = service.Id
                    };

                    context.Employees.Add(employee);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
