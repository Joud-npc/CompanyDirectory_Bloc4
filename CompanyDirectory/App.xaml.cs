using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using CompanyDirectory.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CompanyDirectory
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // --- CONFIGURATION ---
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                var configuration = builder.Build();

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

                using var context = new ApplicationDbContext(optionsBuilder.Options);
                context.Database.EnsureCreated();

                // Seed uniquement si la DB est vide
                if (!context.Employees.Any())
                {
                    await SeedDatabaseAsync(context);
                    MessageBox.Show("Base de données initialisée avec 1000 employés");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur au démarrage : {ex.Message}\n\n{ex.InnerException?.Message}", 
                    "Erreur", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }

            // 🔹 Ouvre ta fenêtre de login
            var login = new LoginWindow();
            login.Show();
        }


        private async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
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

            // Appel API RandomUser pour 1000 employés (en paquets)
            using var http = new HttpClient();
            var rand = new Random();

            int target = 1000;
            int created = 0;

            while (created < target)
            {
                int fetch = Math.Min(100, target - created);
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

                    // Identifiant et mot de passe
                    var username = $"{last}.{first}".ToLower();
                    var password = "azerty123"; // ⚠️ À sécuriser plus tard

                    var site = sites[rand.Next(sites.Count)];
                    var service = services[rand.Next(services.Count)];

                    var employee = new Employee
                    {
                        FirstName = first,
                        LastName = last,
                        Email = email,
                        Phone = phone,
                        SiteId = site.Id,
                        ServiceId = service.Id,
                        Username = username,
                        Password = password
                    };

                    context.Employees.Add(employee);
                    created++;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}