namespace CompanyDirectory.Services
{
    public interface ISeedService
    {
        Task SeedRandomUsersAsync(int count = 1000);
    }
}