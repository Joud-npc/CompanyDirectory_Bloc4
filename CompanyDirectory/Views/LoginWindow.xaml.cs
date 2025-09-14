using System.Windows;

namespace CompanyDirectory.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            // pour tests, n'importe quel user + mot de passe "user" passe
            if (!string.IsNullOrWhiteSpace(UsernameBox.Text) && PasswordBox.Password == "user")
            {
                var main = new MainWindow(); // remplace par ta MainView si nom différent
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Identifiants invalides (démo : mot de passe 'user')");
            }
        }
    }
}