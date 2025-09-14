using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyDirectory.Models;
using CompanyDirectory.Services;
using System.Collections.ObjectModel;

namespace CompanyDirectory.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        private readonly IServiceProvider _sp;

        [ObservableProperty] private ObservableCollection<Employee> employees = new();
        [ObservableProperty] private ObservableCollection<Site> sites = new();
        [ObservableProperty] private ObservableCollection<Service> services = new();
        [ObservableProperty] private string searchText = "";
        [ObservableProperty] private Site? selectedSite;
        [ObservableProperty] private Service? selectedService;

        public IRelayCommand RefreshCommand { get; }

        public MainViewModel(IEmployeeService employeeService, IServiceProvider sp)
        {
            _employeeService = employeeService;
            _sp = sp;
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
        }

        public async Task LoadAsync()
        {
            var list = await _employeeService.GetAllAsync();
            Employees = new ObservableCollection<Employee>(list);
            Sites = new ObservableCollection<Site>(await _employeeService.GetSitesAsync());
            Services = new ObservableCollection<Service>(await _employeeService.GetServicesAsync());
        }

        public IEnumerable<Employee> FilteredEmployees()
        {
            var q = Employees.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.ToLower();
                q = q.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(s) || (e.Email ?? "").ToLower().Contains(s));
            }
            if (SelectedSite != null) q = q.Where(e => e.SiteId == SelectedSite.Id);
            if (SelectedService != null) q = q.Where(e => e.ServiceId == SelectedService.Id);
            return q;
        }
    }
}
