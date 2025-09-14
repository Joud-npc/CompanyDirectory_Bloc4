using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyDirectory.Models;
using CompanyDirectory.Services;
using System.Collections.ObjectModel;

namespace CompanyDirectory.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        public ObservableCollection<Employee> Employees { get; set; } = new();
        public ObservableCollection<Site> Sites { get; set; } = new();
        public ObservableCollection<Service> Services { get; set; } = new();

        [ObservableProperty] private Employee? selectedEmployee;

        public IRelayCommand LoadCommand { get; }
        public IRelayCommand AddCommand { get; }
        public IRelayCommand EditCommand { get; }
        public IRelayCommand DeleteCommand { get; }

        public AdminViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            LoadCommand = new RelayCommand(async () => await LoadAsync());
            AddCommand = new RelayCommand(async () => await AddAsync());
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedEmployee != null);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => SelectedEmployee != null);
        }

        public async Task LoadAsync()
        {
            Employees = new ObservableCollection<Employee>(await _employeeService.GetAllAsync());
            Sites = new ObservableCollection<Site>(await _employeeService.GetSitesAsync());
            Services = new ObservableCollection<Service>(await _employeeService.GetServicesAsync());
            OnPropertyChanged(nameof(Employees));
        }

        private async Task AddAsync()
        {
            var e = new Employee { FirstName = "Nouveau", LastName = "Employe", Email = "nouveau@example.com" };
            await _employeeService.AddAsync(e);
            await LoadAsync();
        }

        private async Task EditAsync()
        {
            if (SelectedEmployee == null) return;
            await _employeeService.UpdateAsync(SelectedEmployee);
            await LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedEmployee == null) return;
            await _employeeService.DeleteAsync(SelectedEmployee.Id);
            await LoadAsync();
        }
    }
}
