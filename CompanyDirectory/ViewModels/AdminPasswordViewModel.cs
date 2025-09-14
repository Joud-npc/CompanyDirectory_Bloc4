using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace CompanyDirectory.ViewModels
{
    public partial class AdminPasswordViewModel : ObservableObject
    {
        public IRelayCommand<PasswordBox> ValidateCommand { get; }

        public AdminPasswordViewModel()
        {
            ValidateCommand = new RelayCommand<PasswordBox>(OnValidate);
        }

        private void OnValidate(PasswordBox passwordBox)
        {
            string password = passwordBox.Password;

            if (password == "secretAdmin") // mot de passe admin
            {
                var adminView = new CompanyDirectory.Views.AdminView();
                adminView.Show();

                // Fermer la fenêtre du mot de passe
                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w is CompanyDirectory.Views.AdminPasswordWindow)
                    ?.Close();
            }
            else
            {
                MessageBox.Show("Mot de passe incorrect", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}