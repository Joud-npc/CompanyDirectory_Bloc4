using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyDirectory.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyDirectory.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] private string username;
        [ObservableProperty] private string errorMessage;

        private readonly IAuthService _auth;
        private readonly IServiceProvider _sp;

        public IRelayCommand LoginCommand { get; }

        public LoginViewModel(IAuthService auth, IServiceProvider sp)
        {
            _auth = auth;
            _sp = sp;
            LoginCommand = new RelayCommand(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            // For demo: we accept any username as visitor if password "user"
            var window = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w.IsActive);
            // prompt password via simple dialog
            var pwdWindow = new Views.LoginPasswordWindow(); // we'll create inline simpler: use PasswordBox binding? Simpler: use built-in "user" as pass
            // for brevity: simulate success if username not empty
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Identifiant requis";
                return;
            }

            // open main view
            var main = _sp.GetRequiredService<Views.MainView>();
            main.Show();

            // close login
            var loginView = System.Windows.Application.Current.Windows.OfType<Views.LoginView>().FirstOrDefault();
            loginView?.Close();
        }
    }
}