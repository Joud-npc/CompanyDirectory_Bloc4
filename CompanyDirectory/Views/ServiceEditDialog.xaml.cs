using System;
using System.Linq;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;

namespace CompanyDirectory.Views
{
    public partial class ServiceEditDialog : Window
    {
        public Service Service { get; private set; }

        public ServiceEditDialog()
        {
            InitializeComponent();
            Service = new Service();
            TitleText.Text = "Nouveau Service";
            TxtNom.Focus();
        }

        public ServiceEditDialog(Service service)
        {
            InitializeComponent();
            Service = new Service { Id = service.Id, Nom = service.Nom };
            TitleText.Text = "Modifier Service";
            TxtNom.Text = service.Nom;
            TxtNom.Focus();
            TxtNom.SelectAll();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                Service.Nom = TxtNom.Text.Trim();

                // Vérifier l'unicité du nom de service
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var existingService = db.Services
                    .FirstOrDefault(s => s.Nom.ToLower() == Service.Nom.ToLower() && s.Id != Service.Id);

                if (existingService != null)
                {
                    MessageBox.Show("Ce service existe déjà.");
                    return;
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}");
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TxtNom.Text))
            {
                MessageBox.Show("Le nom du service est obligatoire.");
                TxtNom.Focus();
                return false;
            }

            // Validation basique du nom de service
            if (TxtNom.Text.Trim().Length < 2)
            {
                MessageBox.Show("Le nom du service doit contenir au moins 2 caractères.");
                TxtNom.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}