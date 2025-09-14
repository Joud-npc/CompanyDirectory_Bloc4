using System.Linq;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory
{
    public partial class MainWindow : Window
    {
        private Employee _currentUser;

        public MainWindow(Employee user)
        {
            InitializeComponent();
            _currentUser = user;

            this.Title = $"Annuaire - Connecté : {_currentUser.Username}";

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
            var employees = db.Employees
                .Include(e => e.Site)
                .Include(e => e.Service)
                .ToList();

            EmployeeGrid.ItemsSource = employees;
        }
    }
}