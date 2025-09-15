using System;
using System.IO;
using System.Windows;
using CompanyDirectory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuestPDF.Infrastructure;

namespace CompanyDirectory
{
    public partial class App : Application
    {
        // Stocke le DbContextOptions pour que toutes les fenêtres puissent l'utiliser
        public DbContextOptions<ApplicationDbContext> DbOptions { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // --- CONFIGURATION ---
            QuestPDF.Settings.License = LicenseType.Community;
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            DbOptions = optionsBuilder.Options;

            // Crée la base si elle n'existe pas
            using var context = new ApplicationDbContext(DbOptions);
            context.Database.EnsureCreated();

            // Lance la fenêtre de login
            var login = new LoginWindow(DbOptions);
            login.Show();
        }
    }
}