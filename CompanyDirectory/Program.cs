using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CompanyDirectory
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current!;
        public IServiceProvider Services { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            // Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.File(Configuration["Serilog:LogFile"] ?? "logs/app-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            Log.Information("Application démarrage");

            var login = Services.GetRequiredService<Views.LoginView>();
            login.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // DbContext
            services.AddDbContext<Data.ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // App services
            services.AddSingleton(Configuration);
            services.AddSingleton<Services.IEmployeeService, Services.EmployeeService>();
            services.AddSingleton<Services.ISeedService, Services.SeedService>();
            services.AddSingleton<Services.IAuthService, Services.AuthService>();
            services.AddSingleton<Services.IPdfService, Services.PdfService>();

            // ViewModels
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.AdminViewModel>();

            // Views
            services.AddTransient<Views.LoginView>();
            services.AddTransient<Views.MainView>();
            services.AddTransient<Views.AdminView>();
            services.AddTransient<Views.AdminPasswordWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application arrêt");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
