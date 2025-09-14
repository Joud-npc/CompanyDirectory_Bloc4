namespace CompanyDirectory.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}