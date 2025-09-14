using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CompanyDirectory.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CompanyDirectory.Views
{
    public partial class MainView : Window
    {
        private readonly MainViewModel _vm;
        private readonly Key[] _secret = new[] {
            Key.Up, Key.Down, Key.Up, Key.Down, Key.Right, Key.Left, Key.Right, Key.Left
        };
        private readonly Queue<Key> _lastKeys = new Queue<Key>();

        public MainView(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.LoadAsync();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _lastKeys.Enqueue(e.Key);
            while (_lastKeys.Count > _secret.Length) _lastKeys.Dequeue();

            if (_lastKeys.Count == _secret.Length && _secret.SequenceEqual(_lastKeys.ToArray()))
            {
                // show admin password prompt
                var pwdWindow = App.Current.Services.GetRequiredService<AdminPasswordWindow>();
                if (pwdWindow.ShowDialog() == true)
                {
                    // admin authenticated
                    Log.Information("Admin unlocked via secret sequence");
                    var adminView = App.Current.Services.GetRequiredService<AdminView>();
                    adminView.Show();
                }
            }
        }
    }
}