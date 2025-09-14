using System.Net.Http;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Newtonsoft.Json.Linq;

namespace CompanyDirectory.Services
{
    public class SeedService : ISeedService
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpClient _http = new HttpClient();

        public SeedService(ApplicationDbContext db) { _db = db; }

        public async Task SeedRandomUsersAsync(int count = 1000)
        {
            // Ensure some base services if none
            if(!_db.Services.Any())
            {
                var defaultServices = new[] { "Production", "Comptabilité", "Accueil", "Maintenance", "R&D" };
                foreach(var s in defaultServices) _db.Services.Add(new Service { Name = s });
            }

            await _db.SaveChangesAsync();

            var url = $"https://randomuser.me/api/?results={count}&nat=fr";
            var json = await _http.GetStringAsync(url);
            var doc = JObject.Parse(json);
            var results = doc["results"] as JArray;
            if (results == null) return;

            var rand = new Random();
            var services = _db.Services.ToList();

            foreach(var r in results)
            {
                var city = (string?)r["location"]?["city"] ?? "Unknown";
                var site = _db.Sites.FirstOrDefault(s => s.City == city);
                if (site == null)
                {
                    site = new Site { City = city };
                    _db.Sites.Add(site);
                    await _db.SaveChangesAsync();
                }

                var first = (string?)r["name"]?["first"] ?? "X";
                var last = (string?)r["name"]?["last"] ?? "X";
                var email = (string?)r["email"] ?? $"{first}.{last}@example.com";
                var phone = (string?)r["phone"] ?? "";
                var cell = (string?)r["cell"] ?? "";

                var service = services[rand.Next(services.Count)];

                var emp = new Employee
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = phone,
                    MobilePhone = cell,
                    SiteId = site.Id,
                    ServiceId = service.Id
                };
                _db.Employees.Add(emp);
            }

            await _db.SaveChangesAsync();
        }
    }
}
