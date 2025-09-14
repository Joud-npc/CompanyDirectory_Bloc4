using System;
using System.Linq;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;

namespace CompanyDirectory.Views
{
    public partial class SiteEditDialog : Window
    {
        public Site Site { get; private set; }

        public SiteEditDialog()
        {
            InitializeComponent();
            Site = new Site();
            TitleText.Text = "Nouveau Site";
            TxtVille.Focus();
        }

        public SiteEditDialog(Site site)
        {
            InitializeComponent();
            Site = new Site { Id = site.Id, Ville = site.Ville };
            TitleText.Text = "Modifier Site";
            TxtVille.Text = site.Ville;
            TxtVille.Focus();
            TxtVille.SelectAll();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                Site.Ville = TxtVille.Text.Trim();

                // Vérifier l'unicité du nom de ville
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var existingSite = db.Sites
                    .FirstOrDefault(s => s.Ville.ToLower() == Site.Ville.ToLower() && s.Id != Site.Id);

                if (existingSite != null)
                {
                    MessageBox.Show("Ce site existe déjà.");
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
            if (string.IsNullOrWhiteSpace(TxtVille.Text))
            {
                MessageBox.Show("Le nom de la ville est obligatoire.");
                TxtVille.Focus();
                return false;
            }

            // Validation basique du nom de ville
            if (TxtVille.Text.Trim().Length < 2)
            {
                MessageBox.Show("Le nom de la ville doit contenir au moins 2 caractères.");
                TxtVille.Focus();
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