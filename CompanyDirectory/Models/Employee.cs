namespace CompanyDirectory.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public int SiteId { get; set; }
        public Site Site { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}