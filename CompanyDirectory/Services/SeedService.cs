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

        public async Task SeedRandomUsersAsync(int count = 50)
        {
            try
            {
                // Initialisation des services si vides
                if (!_db.Services.Any())
                {
                    var defaultServices = new[] 
                    { 
                        "Production", "Comptabilité", "Accueil", 
                        "Maintenance", "R&D", "Ressources Humaines",
                        "Marketing", "Qualité", "Logistique"
                    };
                    
                    foreach (var s in defaultServices)
                    {
                        _db.Services.Add(new Service { Nom = s });
                    }
                    await _db.SaveChangesAsync();
                }

                // Initialisation des sites si vides
                if (!_db.Sites.Any())
                {
                    var defaultSites = new[] 
                    { 
                        "Paris", "Lyon", "Marseille", "Toulouse", 
                        "Bordeaux", "Nantes", "Strasbourg", "Lille"
                    };
                    
                    foreach (var city in defaultSites)
                    {
                        _db.Sites.Add(new Site { Ville = city });
                    }
                    await _db.SaveChangesAsync();
                }

                // Appel à l'API RandomUser
                var url = $"https://randomuser.me/api/?results={count}&nat=fr";
                var json = await _http.GetStringAsync(url);
                var doc = JObject.Parse(json);
                var results = doc["results"] as JArray;
                
                if (results == null) 
                {
                    throw new Exception("Aucune donnée reçue de l'API RandomUser");
                }

                var rand = new Random();
                var services = await _db.Services.ToListAsync();
                var sites = await _db.Sites.ToListAsync();

                foreach (var r in results)
                {
                    try
                    {
                        // Extraction des données de l'API
                        var city = (string?)r["location"]?["city"] ?? "Inconnu";
                        var first = (string?)r["name"]?["first"] ?? "Prénom";
                        var last = (string?)r["name"]?["last"] ?? "Nom";
                        var email = (string?)r["email"] ?? $"{first}.{last}@entreprise.fr";
                        var phone = (string?)r["phone"] ?? "01 23 45 67 89";

                        // Nettoyer le nom de la ville (première lettre majuscule)
                        city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

                        // Vérifier si le site existe déjà, sinon le créer
                        var site = sites.FirstOrDefault(s => s.Ville.Equals(city, StringComparison.OrdinalIgnoreCase));
                        if (site == null)
                        {
                            site = new Site { Ville = city };
                            _db.Sites.Add(site);
                            await _db.SaveChangesAsync();
                            sites.Add(site); // Mettre à jour la liste en mémoire
                        }

                        // Sélectionner un service aléatoire
                        var service = services[rand.Next(services.Count)];

                        // Créer un nom d'utilisateur unique
                        var baseUsername = $"{first.ToLower()}.{last.ToLower()}";
                        var username = baseUsername;
                        int counter = 1;
                        
                        while (_db.Employees.Any(e => e.Username == username))
                        {
                            username = $"{baseUsername}{counter}";
                            counter++;
                        }

                        // Créer l'employé
                        var employee = new Employee
                        {
                            FirstName = char.ToUpper(first[0]) + first.Substring(1),
                            LastName = char.ToUpper(last[0]) + last.Substring(1),
                            Email = email.ToLower(),
                            Phone = phone,
                            Username = username,
                            Password = "password123", // Mot de passe par défaut
                            SiteId = site.Id,
                            ServiceId = service.Id
                        };

                        _db.Employees.Add(employee);
                    }
                    catch (Exception ex)
                    {
                        // Log l'erreur mais continue avec les autres utilisateurs
                        System.Diagnostics.Debug.WriteLine($"Erreur lors de l'import d'un utilisateur : {ex.Message}");
                    }
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'import depuis l'API RandomUser : {ex.Message}");
            }
        }
    }
}