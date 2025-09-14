using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CompanyDirectory.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private EmployeeViewModel selectedEmployee;

        public ObservableCollection<EmployeeViewModel> Employees { get; set; }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand AddCommand { get; }
        public IRelayCommand EditCommand { get; }
        public IRelayCommand DeleteCommand { get; }

        public AdminViewModel()
        {
            Employees = new ObservableCollection<EmployeeViewModel>
            {
                new EmployeeViewModel { FirstName="Jean", LastName="Dupont", Email="jean.dupont@mail.com", Phone="0601020304", Site="Paris", Service="Comptabilité"},
                new EmployeeViewModel { FirstName="Marie", LastName="Martin", Email="marie.martin@mail.com", Phone="0605060708", Site="Lyon", Service="Production"}
            };

            SearchCommand = new RelayCommand(OnSearch);
            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand(OnEdit);
            DeleteCommand = new RelayCommand(OnDelete);
        }

        private void OnSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            var results = Employees.Where(e =>
                e.FirstName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.LastName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.Site.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.Service.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            Employees.Clear();
            foreach (var r in results) Employees.Add(r);
        }

        private void OnAdd()
        {
            MessageBox.Show("Ajouter employé (à implémenter)");
        }

        private void OnEdit()
        {
            if (SelectedEmployee == null) return;
            MessageBox.Show($"Modifier {SelectedEmployee.FirstName} {SelectedEmployee.LastName} (à implémenter)");
        }

        private void OnDelete()
        {
            if (SelectedEmployee == null) return;
            Employees.Remove(SelectedEmployee);
        }
    }
}
