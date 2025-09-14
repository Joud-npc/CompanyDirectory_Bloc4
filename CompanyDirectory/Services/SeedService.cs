using System.Net.Http;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.Services
{
    public class SeedService : ISeedService
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpClient _http = new HttpClient();

        public SeedService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SeedRandomUsersAsync(int count = 1000)
        {
            // Initialisation des services
            if (!_db.Services.Any())
            {
                var defaultServices = new[] { "Production", "Comptabilité", "Accueil", "Maintenance", "R&D" };
                foreach (var s in defaultServices)
                {
                    _db.Services.Add(new Service { Nom = s });
                }
                await _db.SaveChangesAsync();
            }

            // Initialisation des sites (optionnel, peut être rempli par la suite via RandomUser)
            if (!_db.Sites.Any())
            {
                var defaultSites = new[] { "Paris", "Lyon", "Marseille", "Toulouse", "Bordeaux" };
                foreach (var city in defaultSites)
                {
                    _db.Sites.Add(new Site { Ville = city });
                }
                await _db.SaveChangesAsync();
            }

            // Récupération des utilisateurs via RandomUser API
            var url = $"https://randomuser.me/api/?results={count}&nat=fr";
            var json = await _http.GetStringAsync(url);
            var doc = JObject.Parse(json);
            var results = doc["results"] as JArray;
            if (results == null) return;

            var rand = new Random();
            var services = _db.Services.ToList();
            var sites = _db.Sites.ToList();

            foreach (var r in results)
            {
                var city = (string?)r["location"]?["city"] ?? "Unknown";

                // Vérifie si le site existe déjà
                var site = sites.FirstOrDefault(s => s.Ville == city);
                if (site == null)
                {
                    site = new Site { Ville = city };
                    _db.Sites.Add(site);
                    await _db.SaveChangesAsync();
                    sites.Add(site); // Met à jour la liste en mémoire
                }

                var first = (string?)r["name"]?["first"] ?? "X";
                var last = (string?)r["name"]?["last"] ?? "X";
                var email = (string?)r["email"] ?? $"{first}.{last}@example.com";
                var phone = (string?)r["phone"] ?? "";

                var service = services[rand.Next(services.Count)];

                var emp = new Employee
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = phone,
                    SiteId = site.Id,
                    ServiceId = service.Id
                };

                _db.Employees.Add(emp);
            }

            await _db.SaveChangesAsync();
        }
    }
}