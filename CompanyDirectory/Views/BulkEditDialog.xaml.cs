using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.Views
{
    public partial class BulkEditDialog : Window
    {
        public List<Employee> SelectedEmployees { get; private set; }
        public BulkEditChanges Changes { get; private set; }

        private List<Site> _allSites;
        private List<Service> _allServices;

        public BulkEditDialog(List<Employee> selectedEmployees)
        {
            InitializeComponent();
            SelectedEmployees = selectedEmployees;
            Changes = new BulkEditChanges();

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Afficher le nombre d'employés sélectionnés
            TxtSelectedCount.Text = $"{SelectedEmployees.Count} employé(s) sélectionné(s)";

            // Charger les sites et services
            LoadSitesAndServices();

            // Mettre à jour le résumé
            UpdateSummary();
        }

        private void LoadSitesAndServices()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                
                _allSites = db.Sites.OrderBy(s => s.Ville).ToList();
                _allServices = db.Services.OrderBy(s => s.Nom).ToList();

                // Remplir les ComboBox
                CmbNewSite.ItemsSource = _allSites;
                CmbNewService.ItemsSource = _allServices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}");
            }
        }

        #region Événements des CheckBox

        private void ChkUpdateSite_Checked(object sender, RoutedEventArgs e)
        {
            CmbNewSite.IsEnabled = true;
            Changes.UpdateSite = true;
            UpdateSummary();
        }

        private void ChkUpdateSite_Unchecked(object sender, RoutedEventArgs e)
        {
            CmbNewSite.IsEnabled = false;
            CmbNewSite.SelectedIndex = -1;
            Changes.UpdateSite = false;
            Changes.NewSiteId = null;
            UpdateSummary();
        }

        private void ChkUpdateService_Checked(object sender, RoutedEventArgs e)
        {
            CmbNewService.IsEnabled = true;
            Changes.UpdateService = true;
            UpdateSummary();
        }

        private void ChkUpdateService_Unchecked(object sender, RoutedEventArgs e)
        {
            CmbNewService.IsEnabled = false;
            CmbNewService.SelectedIndex = -1;
            Changes.UpdateService = false;
            Changes.NewServiceId = null;
            UpdateSummary();
        }

        private void ChkUpdateEmailDomain_Checked(object sender, RoutedEventArgs e)
        {
            TxtNewEmailDomain.IsEnabled = true;
            Changes.UpdateEmailDomain = true;
            UpdateSummary();
        }

        private void ChkUpdateEmailDomain_Unchecked(object sender, RoutedEventArgs e)
        {
            TxtNewEmailDomain.IsEnabled = false;
            Changes.UpdateEmailDomain = false;
            Changes.NewEmailDomain = null;
            UpdateSummary();
        }

        #endregion

        private void UpdateSummary()
        {
            var summary = new StringBuilder();
            bool hasChanges = false;

            summary.AppendLine($"Modifications qui seront appliquées à {SelectedEmployees.Count} employé(s) :");
            summary.AppendLine();

            if (Changes.UpdateSite && CmbNewSite.SelectedItem is Site selectedSite)
            {
                summary.AppendLine($"• Changer le site vers : {selectedSite.Ville}");
                Changes.NewSiteId = selectedSite.Id;
                hasChanges = true;
            }

            if (Changes.UpdateService && CmbNewService.SelectedItem is Service selectedService)
            {
                summary.AppendLine($"• Changer le service vers : {selectedService.Nom}");
                Changes.NewServiceId = selectedService.Id;
                hasChanges = true;
            }

            if (Changes.UpdateEmailDomain && !string.IsNullOrWhiteSpace(TxtNewEmailDomain.Text))
            {
                summary.AppendLine($"• Changer le domaine email vers : {TxtNewEmailDomain.Text}");
                Changes.NewEmailDomain = TxtNewEmailDomain.Text.Trim();
                hasChanges = true;
            }

            if (!hasChanges)
            {
                summary.AppendLine("Aucune modification sélectionnée");
            }

            TxtSummary.Text = summary.ToString();
            BtnApply.IsEnabled = hasChanges;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            // Validation finale
            if (!ValidateChanges())
                return;

            // Confirmation
            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir appliquer ces modifications à {SelectedEmployees.Count} employé(s) ?\n\n" +
                "Cette action est irréversible.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool ValidateChanges()
        {
            if (Changes.UpdateSite && !Changes.NewSiteId.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner un site.");
                return false;
            }

            if (Changes.UpdateService && !Changes.NewServiceId.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner un service.");
                return false;
            }

            if (Changes.UpdateEmailDomain && string.IsNullOrWhiteSpace(Changes.NewEmailDomain))
            {
                MessageBox.Show("Veuillez saisir un domaine email valide.");
                return false;
            }

            if (Changes.UpdateEmailDomain && !Changes.NewEmailDomain.StartsWith("@"))
            {
                MessageBox.Show("Le domaine email doit commencer par @");
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Événements pour mettre à jour le résumé quand les sélections changent
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            
            // Abonner aux événements de changement de sélection
            CmbNewSite.SelectionChanged += (s, e) => UpdateSummary();
            CmbNewService.SelectionChanged += (s, e) => UpdateSummary();
            TxtNewEmailDomain.TextChanged += (s, e) => UpdateSummary();
        }
    }

    // Classe pour stocker les modifications à appliquer
    public class BulkEditChanges
    {
        public bool UpdateSite { get; set; }
        public int? NewSiteId { get; set; }

        public bool UpdateService { get; set; }
        public int? NewServiceId { get; set; }

        public bool UpdateEmailDomain { get; set; }
        public string NewEmailDomain { get; set; }
    }
}