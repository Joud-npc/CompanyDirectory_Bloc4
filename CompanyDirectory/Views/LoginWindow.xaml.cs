using System;
using System.Linq;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using CompanyDirectory.Services;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory
{
    public partial class LoginWindow : Window
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public LoginWindow(DbContextOptions<ApplicationDbContext> options)
        {
            InitializeComponent();
            _options = options;
            
            // Initialiser les données de test au premier démarrage
            InitializeTestData();
        }

        private async void InitializeTestData()
        {
            try
            {
                using var db = new ApplicationDbContext(_options);
                
                // Vérifier si des données existent déjà
                if (!db.Sites.Any())
                {
                    // Créer quelques sites par défaut
                    var sites = new[]
                    {
                        new Site { Ville = "Paris" },
                        new Site { Ville = "Lyon" },
                        new Site { Ville = "Marseille" },
                        new Site { Ville = "Toulouse" },
                        new Site { Ville = "Bordeaux" }
                    };
                    
                    db.Sites.AddRange(sites);
                    await db.SaveChangesAsync();
                }

                if (!db.Services.Any())
                {
                    // Créer quelques services par défaut
                    var services = new[]
                    {
                        new Service { Nom = "Production" },
                        new Service { Nom = "Comptabilité" },
                        new Service { Nom = "Accueil" },
                        new Service { Nom = "Maintenance" },
                        new Service { Nom = "R&D" },
                        new Service { Nom = "Ressources Humaines" }
                    };
                    
                    db.Services.AddRange(services);
                    await db.SaveChangesAsync();
                }

                // Créer un utilisateur de test si aucun employé n'existe
                if (!db.Employees.Any())
                {
                    var parisSite = db.Sites.First(s => s.Ville == "Paris");  // ✅ Correction ici
                    var accueilService = db.Services.First(s => s.Nom == "Accueil");
                    
                    var testUser = new Employee
                    {
                        FirstName = "Jean",
                        LastName = "Dupont",
                        Email = "jean.dupont@entreprise.fr",
                        Phone = "01 23 45 67 89",
                        Username = "test",
                        Password = "test", // En production, utilisez un hash
                        SiteId = parisSite.Id,  // ✅ Correction ici aussi
                        ServiceId = accueilService.Id
                    };
                    
                    db.Employees.Add(testUser);
                    await db.SaveChangesAsync();
                    
                    // Importer quelques utilisateurs via l'API RandomUser
                    var seedService = new SeedService(db);
                    await seedService.SeedRandomUsersAsync(50);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation des données : {ex.Message}");
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez saisir un nom d'utilisateur et un mot de passe.", 
                    "Champs manquants", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new ApplicationDbContext(_options);

                var user = db.Employees
                    .Include(e => e.Site)
                    .Include(e => e.Service)
                    .FirstOrDefault(emp => emp.Username == username && emp.Password == password);

                if (user != null)
                {
                    // Log de la connexion
                    LogConnection(user, true);
                    
                    var mainWindow = new MainWindow(user);
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Log de la tentative échouée
                    LogConnection(null, false, username);
                    
                    MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur de connexion",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion : {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogConnection(Employee user, bool success, string attemptedUsername = "")
        {
            try
            {
                var logPath = System.IO.Path.Combine(AppContext.BaseDirectory, "logs", "connections.log");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));

                string logEntry;
                if (success)
                {
                    logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CONNEXION REUSSIE - {user.FirstName} {user.LastName} ({user.Username})\n";
                }
                else
                {
                    logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TENTATIVE ECHOUEE - Username: {attemptedUsername}\n";
                }

                System.IO.File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignorer les erreurs de logging
            }
        }

        // Événement pour gérer la touche Entrée dans le champ mot de passe
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && TxtPassword.IsFocused)
            {
                BtnLogin_Click(this, new RoutedEventArgs());
            }
            base.OnKeyDown(e);
        }
    }
}