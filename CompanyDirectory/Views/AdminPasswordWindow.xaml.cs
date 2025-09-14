using System.Windows;
using System.Windows.Input;

namespace CompanyDirectory.Views
{
    public partial class AdminPasswordWindow : Window
    {
        private const string ADMIN_PASSWORD = "admin123"; // En production, utilisez un hash sécurisé
        private int _attemptCount = 0;
        private const int MAX_ATTEMPTS = 3;

        public AdminPasswordWindow()
        {
            InitializeComponent();
            PasswordBox.Focus();
        }

        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            ValidatePassword();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidatePassword();
            }
        }

        private void ValidatePassword()
        {
            string password = PasswordBox.Password;
            
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Veuillez saisir un mot de passe.");
                return;
            }

            if (password == ADMIN_PASSWORD)
            {
                // Authentification réussie
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                _attemptCount++;
                
                if (_attemptCount >= MAX_ATTEMPTS)
                {
                    MessageBox.Show("Nombre maximum de tentatives atteint.\nAccès refusé.", 
                        "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.DialogResult = false;
                    this.Close();
                }
                else
                {
                    int remaining = MAX_ATTEMPTS - _attemptCount;
                    ShowError($"Mot de passe incorrect. {remaining} tentative(s) restante(s).");
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}