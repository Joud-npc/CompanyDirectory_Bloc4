using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CompanyDirectory.Models;
using CompanyDirectory.Services;
using Microsoft.Win32;

namespace CompanyDirectory.Views
{
    public partial class EmployeeDetailWindow : Window
    {
        private readonly Employee _employee;
        private readonly PdfService _pdfService;

        public EmployeeDetailWindow(Employee employee)
        {
            InitializeComponent();
            _employee = employee;
            _pdfService = new PdfService();
            
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            // En-tête
            TxtFullName.Text = $"{_employee.FirstName} {_employee.LastName}";
            TxtPosition.Text = $"{_employee.Service?.Nom ?? "N/A"} - {_employee.Site?.Ville ?? "N/A"}";

            // Détails
            TxtLastName.Text = _employee.LastName;
            TxtFirstName.Text = _employee.FirstName;
            TxtEmail.Text = _employee.Email;
            TxtPhone.Text = _employee.Phone;
            TxtSite.Text = _employee.Site?.Ville ?? "N/A";
            TxtService.Text = _employee.Service?.Nom ?? "N/A";

            // Événement clic sur email pour ouvrir le client mail
            TxtEmail.MouseLeftButtonDown += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo($"mailto:{_employee.Email}") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir le client mail : {ex.Message}", 
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
        }

        private void BtnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Boîte de dialogue pour choisir l'emplacement de sauvegarde
                var saveDialog = new SaveFileDialog
                {
                    Title = "Enregistrer la fiche employé",
                    Filter = "Fichiers PDF (*.pdf)|*.pdf",
                    FileName = $"Fiche_{_employee.LastName}_{_employee.FirstName}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Générer le PDF
                    _pdfService.GenerateEmployeePdf(_employee, saveDialog.FileName);
                    
                    var result = MessageBox.Show(
                        $"Fiche PDF générée avec succès !\n\nEmplacement : {saveDialog.FileName}\n\nVoulez-vous l'ouvrir maintenant ?",
                        "Succès", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Ouvrir le PDF avec le programme par défaut
                        Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la génération du PDF :\n{ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}