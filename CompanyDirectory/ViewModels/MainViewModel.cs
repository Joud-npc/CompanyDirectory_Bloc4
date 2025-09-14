using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompanyDirectory.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string searchText = string.Empty;

        public ObservableCollection<EmployeeViewModel> Employees { get; set; }

        public IRelayCommand SearchCommand { get; }

        public MainViewModel()
        {
            // 🔹 Exemple de données provisoires (plus tard ce sera EF + PostgreSQL)
            Employees = new ObservableCollection<EmployeeViewModel>
            {
                new EmployeeViewModel { FirstName="Jean", LastName="Dupont", Email="jean.dupont@mail.com", Phone="0601020304", Site="Paris", Service="Comptabilité"},
                new EmployeeViewModel { FirstName="Marie", LastName="Martin", Email="marie.martin@mail.com", Phone="0605060708", Site="Lyon", Service="Production"}
            };

            SearchCommand = new RelayCommand(OnSearch);
        }

        private void OnSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            var results = Employees.Where(e =>
                e.FirstName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.LastName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.Site.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                e.Service.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            Employees.Clear();
            foreach (var r in results)
                Employees.Add(r);
        }
    }

    public class EmployeeViewModel : ObservableObject
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
    }
}
