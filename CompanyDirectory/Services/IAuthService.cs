namespace CompanyDirectory.Services
{
    public interface IAuthService
    {
        Task<bool> AuthenticateAsync(string username, string password);
        Task<bool> ValidateAdminPasswordAsync(string password);
    }
}