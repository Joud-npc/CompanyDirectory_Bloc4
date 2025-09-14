using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace CompanyDirectory.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string username = string.Empty;

        public IRelayCommand<PasswordBox> LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(OnLogin);
        }

        private void OnLogin(PasswordBox passwordBox)
        {
            string password = passwordBox.Password;

            // ✅ Exemple simplifié
            if (Username == "admin" && password == "secret")
            {
                MessageBox.Show("Connexion admin réussie !");
                var mainView = new CompanyDirectory.Views.MainView();
                mainView.Show();

                // Fermer la fenêtre de login
                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w is CompanyDirectory.Views.LoginView)
                    ?.Close();
            }
        }
    }
}