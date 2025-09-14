using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using CompanyDirectory.Views;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory
{
    public partial class MainWindow : Window
    {
        private Employee _currentUser;
        private List<Employee> _allEmployees;
        private List<Site> _allSites;
        private List<Service> _allServices;
        
        // Gestion du code secret : ↑↓↑↓→←→←
        private readonly List<Key> _keySequence = new();
        private readonly Key[] _secretKeys = { 
            Key.Up, Key.Down, Key.Up, Key.Down, 
            Key.Right, Key.Left, Key.Right, Key.Left 
        };

        public MainWindow(Employee user)
        {
            InitializeComponent();
            _currentUser = user;
            
            // Configuration interface
            this.Title = $"Annuaire - Entreprise Agroalimentaire";
            UserText.Text = $"{_currentUser.FirstName} {_currentUser.LastName}";
            
            // Focus sur la fenêtre pour capturer les touches
            this.Focusable = true;
            this.Focus();
            
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                
                // Charger tous les employés avec leurs relations
                _allEmployees = db.Employees
                    .Include(e => e.Site)
                    .Include(e => e.Service)
                    .ToList();

                // Charger sites et services pour les ComboBox
                _allSites = db.Sites.OrderBy(s => s.Ville).ToList();
                _allServices = db.Services.OrderBy(s => s.Nom).ToList();

                // Remplir les ComboBox
                CmbSite.Items.Clear();
                CmbSite.Items.Add(new Site { Id = 0, Ville = "Tous les sites" });
                foreach (var site in _allSites)
                    CmbSite.Items.Add(site);
                CmbSite.SelectedIndex = 0;

                CmbService.Items.Clear();
                CmbService.Items.Add(new Service { Id = 0, Nom = "Tous les services" });
                foreach (var service in _allServices)
                    CmbService.Items.Add(service);
                CmbService.SelectedIndex = 0;

                // Afficher tous les employés
                RefreshEmployeeGrid(_allEmployees);
                StatusText.Text = "Données chargées avec succès";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Erreur de chargement";
            }
        }

        private void RefreshEmployeeGrid(List<Employee> employees)
        {
            EmployeeGrid.ItemsSource = employees;
            CountText.Text = $"{employees.Count} employé(s)";
        }

        private void ApplyFilters()
        {
            var filteredEmployees = _allEmployees.AsEnumerable();

            // Filtre par nom/prénom
            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                string searchTerm = TxtSearch.Text.ToLower();
                filteredEmployees = filteredEmployees.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm));
            }

            // Filtre par site
            if (CmbSite.SelectedItem is Site selectedSite && selectedSite.Id > 0)
            {
                filteredEmployees = filteredEmployees.Where(e => e.SiteId == selectedSite.Id);
            }

            // Filtre par service
            if (CmbService.SelectedItem is Service selectedService && selectedService.Id > 0)
            {
                filteredEmployees = filteredEmployees.Where(e => e.ServiceId == selectedService.Id);
            }

            RefreshEmployeeGrid(filteredEmployees.ToList());
        }

        // Événements de recherche
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = "";
            CmbSite.SelectedIndex = 0;
            CmbService.SelectedIndex = 0;
            RefreshEmployeeGrid(_allEmployees);
        }

        // Affichage des détails d'un employé
        private void EmployeeGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EmployeeGrid.SelectedItem is Employee selectedEmployee)
            {
                ShowEmployeeDetails(selectedEmployee);
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int employeeId)
            {
                var employee = _allEmployees.FirstOrDefault(emp => emp.Id == employeeId);
                if (employee != null)
                {
                    ShowEmployeeDetails(employee);
                }
            }
        }

        private void ShowEmployeeDetails(Employee employee)
        {
            var detailWindow = new EmployeeDetailWindow(employee);
            detailWindow.Owner = this;
            detailWindow.ShowDialog();
        }

        // Gestion du code secret pour l'admin
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            _keySequence.Add(e.Key);

            // Garder seulement les 8 dernières touches
            if (_keySequence.Count > 8)
            {
                _keySequence.RemoveAt(0);
            }

            // Vérifier si la séquence correspond
            if (_keySequence.Count == 8 && _keySequence.SequenceEqual(_secretKeys))
            {
                _keySequence.Clear();
                OpenAdminMode();
            }
        }

        private void OpenAdminMode()
        {
            var adminPasswordWindow = new AdminPasswordWindow();
            adminPasswordWindow.Owner = this;
            
            if (adminPasswordWindow.ShowDialog() == true)
            {
                // Mode admin activé
                ModeText.Text = "Mode : Administrateur";
                var adminWindow = new AdminManagementWindow(_currentUser);
                adminWindow.Owner = this;
                adminWindow.Show();
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            Application.Current.Shutdown();
            base.OnClosed(e);
        }
    }
}