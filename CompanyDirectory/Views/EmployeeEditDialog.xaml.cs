using System;
using System.Linq;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.Views
{
    public partial class EmployeeEditDialog : Window
    {
        public Employee Employee { get; private set; }
        private readonly bool _isEditMode;

        public EmployeeEditDialog()
        {
            InitializeComponent();
            _isEditMode = false;
            Employee = new Employee();
            TitleText.Text = "Nouvel Employé";
            LoadComboBoxes();
        }

        public EmployeeEditDialog(Employee employee)
        {
            InitializeComponent();
            _isEditMode = true;
            Employee = new Employee
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.Phone,
                Username = employee.Username,
                Password = employee.Password,
                SiteId = employee.SiteId,
                ServiceId = employee.ServiceId
            };
            
            TitleText.Text = "Modifier Employé";
            LoadComboBoxes();
            LoadEmployeeData();
        }

        private void LoadComboBoxes()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                
                // Charger les sites
                var sites = db.Sites.OrderBy(s => s.Ville).ToList();
                CmbSite.ItemsSource = sites;

                // Charger les services
                var services = db.Services.OrderBy(s => s.Nom).ToList();
                CmbService.ItemsSource = services;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}");
            }
        }

        private void LoadEmployeeData()
        {
            TxtLastName.Text = Employee.LastName;
            TxtFirstName.Text = Employee.FirstName;
            TxtEmail.Text = Employee.Email;
            TxtPhone.Text = Employee.Phone;
            TxtUsername.Text = Employee.Username;
            // Ne pas charger le mot de passe pour des raisons de sécurité
            
            CmbSite.SelectedValue = Employee.SiteId;
            CmbService.SelectedValue = Employee.ServiceId;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // Mise à jour des propriétés
                Employee.LastName = TxtLastName.Text.Trim();
                Employee.FirstName = TxtFirstName.Text.Trim();
                Employee.Email = TxtEmail.Text.Trim();
                Employee.Phone = TxtPhone.Text.Trim();
                Employee.Username = TxtUsername.Text.Trim();
                
                // Mise à jour du mot de passe seulement s'il a été saisi
                if (!string.IsNullOrWhiteSpace(TxtPassword.Password))
                {
                    Employee.Password = TxtPassword.Password; // En production, utilisez un hash
                }
                else if (!_isEditMode)
                {
                    MessageBox.Show("Le mot de passe est obligatoire pour un nouvel employé.");
                    return;
                }

                Employee.SiteId = (int)CmbSite.SelectedValue;
                Employee.ServiceId = (int)CmbService.SelectedValue;

                // Vérifier l'unicité du nom d'utilisateur
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                var existingUser = db.Employees
                    .FirstOrDefault(e => e.Username == Employee.Username && e.Id != Employee.Id);
                
                if (existingUser != null)
                {
                    MessageBox.Show("Ce nom d'utilisateur est déjà utilisé.");
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
            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                MessageBox.Show("Le nom est obligatoire.");
                TxtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("Le prénom est obligatoire.");
                TxtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                MessageBox.Show("L'email est obligatoire.");
                TxtEmail.Focus();
                return false;
            }

            // Validation basique de l'email
            if (!TxtEmail.Text.Contains("@") || !TxtEmail.Text.Contains("."))
            {
                MessageBox.Show("L'email n'est pas valide.");
                TxtEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtPhone.Text))
            {
                MessageBox.Show("Le téléphone est obligatoire.");
                TxtPhone.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                MessageBox.Show("Le nom d'utilisateur est obligatoire.");
                TxtUsername.Focus();
                return false;
            }

            if (CmbSite.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un site.");
                CmbSite.Focus();
                return false;
            }

            if (CmbService.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un service.");
                CmbService.Focus();
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