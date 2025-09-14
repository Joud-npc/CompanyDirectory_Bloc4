using System.Windows;
using CompanyDirectory.Services;

namespace CompanyDirectory.Views
{
    public partial class AdminPasswordWindow : Window
    {
        private readonly IAuthService _auth;
        public AdminPasswordWindow(IAuthService auth)
        {
            InitializeComponent();
            _auth = auth;
        }

        private async void Ok_Click(object sender, RoutedEventArgs e)
        {
            var pwd = PwdBox.Password;
            var ok = await _auth.ValidateAdminPasswordAsync(pwd);
            if (ok)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Mot de passe invalide", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}