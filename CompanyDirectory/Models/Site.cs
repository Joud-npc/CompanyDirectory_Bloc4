namespace CompanyDirectory.Models
{
    public class Site
    {
        public int Id { get; set; }
        public string City { get; set; } = "";
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}