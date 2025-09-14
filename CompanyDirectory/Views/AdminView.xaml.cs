using System.Windows;
using CompanyDirectory.ViewModels;

namespace CompanyDirectory.Views
{
    public partial class AdminView : Window
    {
        private readonly AdminViewModel _vm;
        public AdminView(AdminViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.LoadAsync();
        }
    }
}