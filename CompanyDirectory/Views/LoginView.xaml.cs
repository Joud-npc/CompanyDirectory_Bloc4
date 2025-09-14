using System.Windows;
using CompanyDirectory.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyDirectory.Views
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _vm;
        public LoginView(LoginViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            // quick demo: accept any username with password "user"
            var pwd = PasswordBox.Password;
            if (string.IsNullOrWhiteSpace(_vm.Username) || pwd != "user")
            {
                _vm.ErrorMessage = "Identifiants invalides (demo: username + user)";
                return;
            }

            var main = App.Current.Services.GetRequiredService<MainView>();
            main.Show();
            this.Close();
        }
    }
}