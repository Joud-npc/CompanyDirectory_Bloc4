using System;
using System.Collections.Generic;
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
            this.Title = "Ajouter un employé";
            LoadSitesAndServices();
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
            this.Title = "Modifier un employé";
            LoadSitesAndServices();
            PopulateFields();
        }

        private void LoadSitesAndServices()
        {
            try
            {
                using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                
                var sites = db.Sites.OrderBy(s => s.Ville).ToList();
                var services = db.Services.OrderBy(s => s.Nom).ToList();

                CmbSite.ItemsSource = sites;
                CmbService.ItemsSource = services;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}");
            }
        }

        private void PopulateFields()
        {
            TxtFirstName.Text = Employee.FirstName;
            TxtLastName.Text = Employee.LastName;
            TxtEmail.Text = Employee.Email;
            TxtPhone.Text = Employee.Phone;
            TxtUsername.Text = Employee.Username;

            CmbSite.SelectedValue = Employee.SiteId;
            CmbService.SelectedValue = Employee.ServiceId;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                Employee.FirstName = TxtFirstName.Text.Trim();
                Employee.LastName = TxtLastName.Text.Trim();
                Employee.Email = TxtEmail.Text.Trim();
                Employee.Phone = TxtPhone.Text.Trim();
                Employee.Username = TxtUsername.Text.Trim();
                Employee.SiteId = (int)CmbSite.SelectedValue;
                Employee.ServiceId = (int)CmbService.SelectedValue;

                // Si c'est un nouvel employé, définir un mot de passe par défaut
                if (!_isEditMode)
                {
                    Employee.Password = "password123";
                }

                this.DialogResult = true;
                this.Close();
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("Le prénom est obligatoire.");
                TxtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                MessageBox.Show("Le nom est obligatoire.");
                TxtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtEmail.Text) || !TxtEmail.Text.Contains("@"))
            {
                MessageBox.Show("L'email doit être valide.");
                TxtEmail.Focus();
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
                return false;
            }

            if (CmbService.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un service.");
                return false;
            }

            // Vérifier l'unicité du nom d'utilisateur
            if (!_isEditMode || TxtUsername.Text.Trim() != Employee.Username)
            {
                try
                {
                    using var db = new ApplicationDbContext(((App)Application.Current).DbOptions);
                    bool usernameExists = db.Employees.Any(e => e.Username == TxtUsername.Text.Trim() && e.Id != Employee.Id);
                    if (usernameExists)
                    {
                        MessageBox.Show("Ce nom d'utilisateur existe déjà.");
                        TxtUsername.Focus();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la validation : {ex.Message}");
                    return false;
                }
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