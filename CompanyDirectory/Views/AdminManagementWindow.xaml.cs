using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using CompanyDirectory.Services;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.Views
{
    public partial class AdminManagementWindow : Window
    {
        private readonly Employee _currentUser;
        private readonly SeedService _seedService;
        private readonly string _logFilePath;
        
        public AdminManagementWindow(Employee currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "admin_access.log");
            
            // Créer le dossier logs s'il n'existe pas
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
            
            // Initialiser le service de données
            using var context = new ApplicationDbContext(((App)Application.Current).DbOptions);
            _seedService = new SeedService(context);
            
            // Logger l'accès admin
            LogAdminAccess($"Accès administrateur - Utilisateur: {_currentUser.FirstName} {_currentUser.LastName}");
            
            LoadAllData();
        }

        #region Chargement des données
        
        private void LoadAllData()
        {
            LoadEmployees();
            LoadSites();
            LoadServices();
            LoadLogs();
        }

        private void LoadEmployees()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var employees = db.Employees
                    .Include(e => e.Site)
                    .Include(e => e.Service)
                    .OrderBy(e => e.LastName)
                    .ToList();
                
                EmployeesGrid.ItemsSource = employees;
                EmployeesCountText.Text = $"{employees.Count} employé(s)";
                EmployeesStatusText.Text = "Employés chargés";
                
                // Mettre à jour le compteur de sélection
                UpdateSelectionCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des employés : {ex.Message}");
                LogError($"Erreur chargement employés: {ex.Message}");
            }
        }

        private void LoadSites()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var sites = db.Sites.OrderBy(s => s.Ville).ToList();
                
                SitesGrid.ItemsSource = sites;
                SitesCountText.Text = $"{sites.Count} site(s)";
                SitesStatusText.Text = "Sites chargés";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des sites : {ex.Message}");
                LogError($"Erreur chargement sites: {ex.Message}");
            }
        }

        private void LoadServices()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var services = db.Services.OrderBy(s => s.Nom).ToList();
                
                ServicesGrid.ItemsSource = services;
                ServicesCountText.Text = $"{services.Count} service(s)";
                ServicesStatusText.Text = "Services chargés";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des services : {ex.Message}");
                LogError($"Erreur chargement services: {ex.Message}");
            }
        }
        
        #endregion

        #region Gestion de la sélection multiple

        private void EmployeesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            int selectedCount = EmployeesGrid.SelectedItems.Count;
            TxtSelectionCount.Text = $"{selectedCount} sélectionné(s)";
            
            // Activer/désactiver les boutons de modification multiple
            BtnBulkEdit.IsEnabled = selectedCount > 0;
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            EmployeesGrid.SelectAll();
        }

        private void BtnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            EmployeesGrid.UnselectAll();
        }

        private void BtnBulkEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployees = GetSelectedEmployees();
            if (selectedEmployees.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins un employé.");
                return;
            }

            var bulkEditDialog = new BulkEditDialog(selectedEmployees);
            bulkEditDialog.Owner = this;
            
            if (bulkEditDialog.ShowDialog() == true)
            {
                try
                {
                    ApplyBulkChanges(selectedEmployees, bulkEditDialog.Changes);
                    LoadEmployees(); // Recharger la grille
                    MessageBox.Show($"Modifications appliquées avec succès à {selectedEmployees.Count} employé(s) !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'application des modifications : {ex.Message}");
                    LogError($"Erreur modification multiple: {ex.Message}");
                }
            }
        }

        private List<Employee> GetSelectedEmployees()
        {
            return EmployeesGrid.SelectedItems.Cast<Employee>().ToList();
        }

        private void ApplyBulkChanges(List<Employee> employees, BulkEditChanges changes)
        {
            using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
            
            int modifiedCount = 0;
            var modifications = new List<string>();

            foreach (var employee in employees)
            {
                var dbEmployee = db.Employees.Find(employee.Id);
                if (dbEmployee != null)
                {
                    bool employeeModified = false;

                    // Modification du site
                    if (changes.UpdateSite && changes.NewSiteId.HasValue)
                    {
                        var oldSiteId = dbEmployee.SiteId;
                        dbEmployee.SiteId = changes.NewSiteId.Value;
                        employeeModified = true;
                    }
                        
                    // Modification du service
                    if (changes.UpdateService && changes.NewServiceId.HasValue)
                    {
                        var oldServiceId = dbEmployee.ServiceId;
                        dbEmployee.ServiceId = changes.NewServiceId.Value;
                        employeeModified = true;
                    }

                    // Modification du domaine email
                    if (changes.UpdateEmailDomain && !string.IsNullOrWhiteSpace(changes.NewEmailDomain))
                    {
                        var emailParts = dbEmployee.Email.Split('@');
                        if (emailParts.Length == 2)
                        {
                            var newDomain = changes.NewEmailDomain.StartsWith("@") ? 
                                changes.NewEmailDomain.Substring(1) : changes.NewEmailDomain;
                            dbEmployee.Email = $"{emailParts[0]}@{newDomain}";
                            employeeModified = true;
                        }
                    }

                    if (employeeModified)
                    {
                        modifiedCount++;
                        modifications.Add($"{dbEmployee.FirstName} {dbEmployee.LastName}");
                    }
                }
            }
            
            db.SaveChanges();
            
            // Log des modifications
            var logMessage = $"Modification en lot appliquée à {modifiedCount} employé(s)";
            if (changes.UpdateSite) logMessage += $" - Nouveau site ID: {changes.NewSiteId}";
            if (changes.UpdateService) logMessage += $" - Nouveau service ID: {changes.NewServiceId}";
            if (changes.UpdateEmailDomain) logMessage += $" - Nouveau domaine: {changes.NewEmailDomain}";
            
            LogAdminAccess(logMessage);
        }

        #endregion

        #region Gestion des Employés

        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EmployeeEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                    db.Employees.Add(dialog.Employee);
                    db.SaveChanges();
                    
                    LogAdminAccess($"Employé ajouté: {dialog.Employee.FirstName} {dialog.Employee.LastName}");
                    LoadEmployees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}");
                    LogError($"Erreur ajout employé: {ex.Message}");
                }
            }
        }

        private void BtnEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesGrid.SelectedItem is Employee selectedEmployee)
            {
                var dialog = new EmployeeEditDialog(selectedEmployee);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                        db.Employees.Update(dialog.Employee);
                        db.SaveChanges();
                        
                        LogAdminAccess($"Employé modifié: {dialog.Employee.FirstName} {dialog.Employee.LastName}");
                        LoadEmployees();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la modification : {ex.Message}");
                        LogError($"Erreur modification employé: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un employé à modifier.");
            }
        }

        private void BtnDeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployees = GetSelectedEmployees();
            
            if (selectedEmployees.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins un employé à supprimer.");
                return;
            }

            string message = selectedEmployees.Count == 1 ?
                $"Êtes-vous sûr de vouloir supprimer {selectedEmployees[0].FirstName} {selectedEmployees[0].LastName} ?" :
                $"Êtes-vous sûr de vouloir supprimer les {selectedEmployees.Count} employés sélectionnés ?";

            var result = MessageBox.Show(message, "Confirmation de suppression",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                    
                    foreach (var selectedEmployee in selectedEmployees)
                    {
                        var employee = db.Employees.Find(selectedEmployee.Id);
                        if (employee != null)
                        {
                            db.Employees.Remove(employee);
                            LogAdminAccess($"Employé supprimé: {selectedEmployee.FirstName} {selectedEmployee.LastName}");
                        }
                    }
                    
                    db.SaveChanges();
                    LoadEmployees();
                    MessageBox.Show($"{selectedEmployees.Count} employé(s) supprimé(s) avec succès.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression : {ex.Message}");
                    LogError($"Erreur suppression employé: {ex.Message}");
                }
            }
        }

        private async void BtnImportUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(TxtImportCount.Text, out int count) || count <= 0 || count > 1000)
                {
                    MessageBox.Show("Veuillez saisir un nombre valide entre 1 et 1000.");
                    return;
                }

                BtnImportUsers.IsEnabled = false;
                BtnImportUsers.Content = "Import en cours...";

                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var seedService = new SeedService(db);
                await seedService.SeedRandomUsersAsync(count);

                LogAdminAccess($"Import API: {count} utilisateurs importés");
                LoadEmployees();
                MessageBox.Show($"{count} utilisateurs importés avec succès !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import : {ex.Message}");
                LogError($"Erreur import API: {ex.Message}");
            }
            finally
            {
                BtnImportUsers.IsEnabled = true;
                BtnImportUsers.Content = "Importer via API";
            }
        }

        #endregion

        #region Gestion des Sites

        private void BtnAddSite_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SiteEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                    db.Sites.Add(dialog.Site);
                    db.SaveChanges();
                    
                    LogAdminAccess($"Site ajouté: {dialog.Site.Ville}");
                    LoadSites();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}");
                    LogError($"Erreur ajout site: {ex.Message}");
                }
            }
        }

        private void BtnEditSite_Click(object sender, RoutedEventArgs e)
        {
            if (SitesGrid.SelectedItem is Site selectedSite)
            {
                var dialog = new SiteEditDialog(selectedSite);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                        db.Sites.Update(dialog.Site);
                        db.SaveChanges();
                        
                        LogAdminAccess($"Site modifié: {dialog.Site.Ville}");
                        LoadSites();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la modification : {ex.Message}");
                        LogError($"Erreur modification site: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un site à modifier.");
            }
        }

        private void BtnDeleteSite_Click(object sender, RoutedEventArgs e)
        {
            if (SitesGrid.SelectedItem is Site selectedSite)
            {
                // Vérifier s'il y a des employés dans ce site
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var employeeCount = db.Employees.Count(emp => emp.SiteId == selectedSite.Id);
                
                if (employeeCount > 0)
                {
                    MessageBox.Show($"Impossible de supprimer ce site car {employeeCount} employé(s) y sont affecté(s).");
                    return;
                }

                var result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer le site {selectedSite.Ville} ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var site = db.Sites.Find(selectedSite.Id);
                        if (site != null)
                        {
                            db.Sites.Remove(site);
                            db.SaveChanges();
                            
                            LogAdminAccess($"Site supprimé: {selectedSite.Ville}");
                            LoadSites();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression : {ex.Message}");
                        LogError($"Erreur suppression site: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un site à supprimer.");
            }
        }

        #endregion

        #region Gestion des Services

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ServiceEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                    db.Services.Add(dialog.Service);
                    db.SaveChanges();
                    
                    LogAdminAccess($"Service ajouté: {dialog.Service.Nom}");
                    LoadServices();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}");
                    LogError($"Erreur ajout service: {ex.Message}");
                }
            }
        }

        private void BtnEditService_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesGrid.SelectedItem is Service selectedService)
            {
                var dialog = new ServiceEditDialog(selectedService);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                        db.Services.Update(dialog.Service);
                        db.SaveChanges();
                        
                        LogAdminAccess($"Service modifié: {dialog.Service.Nom}");
                        LoadServices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la modification : {ex.Message}");
                        LogError($"Erreur modification service: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un service à modifier.");
            }
        }

        private void BtnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesGrid.SelectedItem is Service selectedService)
            {
                // Vérifier s'il y a des employés dans ce service
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var employeeCount = db.Employees.Count(emp => emp.ServiceId == selectedService.Id);
                
                if (employeeCount > 0)
                {
                    MessageBox.Show($"Impossible de supprimer ce service car {employeeCount} employé(s) y sont affecté(s).");
                    return;
                }

                var result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer le service {selectedService.Nom} ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var service = db.Services.Find(selectedService.Id);
                        if (service != null)
                        {
                            db.Services.Remove(service);
                            db.SaveChanges();
                            
                            LogAdminAccess($"Service supprimé: {selectedService.Nom}");
                            LoadServices();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression : {ex.Message}");
                        LogError($"Erreur suppression service: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un service à supprimer.");
            }
        }

        #endregion

        #region Gestion des Logs

        private void LoadLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    var logs = File.ReadAllText(_logFilePath);
                    TxtLogs.Text = logs;
                    TxtLogs.ScrollToEnd();
                }
                else
                {
                    TxtLogs.Text = "Aucun log disponible.";
                }
            }
            catch (Exception ex)
            {
                TxtLogs.Text = $"Erreur lors du chargement des logs : {ex.Message}";
            }
        }

        private void BtnRefreshLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void BtnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir vider tous les logs ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText(_logFilePath, string.Empty);
                    LoadLogs();
                    LogAdminAccess("Logs vidés par l'administrateur");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression des logs : {ex.Message}");
                }
            }
        }

        private void LogAdminAccess(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch
            {
                // Ignorer les erreurs de logging pour ne pas planter l'application
            }
        }

        private void LogError(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERREUR: {message}\n";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch
            {
                // Ignorer les erreurs de logging
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            LogAdminAccess($"Fermeture session admin - Utilisateur: {_currentUser.FirstName} {_currentUser.LastName}");
            base.OnClosed(e);
        }
    }
}