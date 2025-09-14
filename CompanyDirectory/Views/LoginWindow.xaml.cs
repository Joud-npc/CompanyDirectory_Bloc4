using System.Linq;
using System.Windows;
using CompanyDirectory.Data;
using CompanyDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory
{
    public partial class LoginWindow : Window
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public LoginWindow(DbContextOptions<ApplicationDbContext> options)
        {
            InitializeComponent();
            _options = options;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            using var db = new ApplicationDbContext(_options);

            var user = db.Employees
                .FirstOrDefault(emp => emp.Username == username && emp.Password == password);

            if (user != null)
            {
                var mainWindow = new MainWindow(user);
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Identifiant ou mot de passe incorrect.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}