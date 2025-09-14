using System;
using System.IO;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CompanyDirectory
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Configuration["Serilog:LogFile"] ?? "logs/app-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            Log.Information("Application start");

            var login = Services.GetRequiredService<Views.LoginView>();
            login.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<IEmployeeService, EmployeeService>();
            services.AddSingleton<ISeedService, SeedService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IPdfService, PdfService>();

            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.AdminViewModel>();

            services.AddTransient<Views.LoginView>();
            services.AddTransient<Views.MainView>();
            services.AddTransient<Views.AdminView>();
            services.AddTransient<Views.AdminPasswordWindow>();

            services.AddTransient<Helpers.HashGenerator>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application exit");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
