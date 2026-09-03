using FikirHavuzu.Entity.Entities;

namespace FikirHavuzu.Business.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string registrationNumber, string password);
        Task<bool> RegisterUserAsync(User user, string plainPassword);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> AssignPermissionToUserAsync(int userId, int permissionId, int grantedByUserId);
        Task<bool> HasPermissionAsync(int userId, string permissionName);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> UpdateUserAsync(User user);
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<List<int>> GetUserPermissionIdsAsync(int userId);
        Task<bool> UpdateUserPermissionsAsync(int userId, List<int> selectedPermissionIds, int grantedByUserId);
        
        Task<string> GenerateUniqueRegistrationNumberAsync(DateTime registrationDate);
        Task<bool> SavePasswordResetTokenAsync(string email, string token, DateTime expiration);
        Task<User?> ValidatePasswordResetTokenAsync(string email, string token);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> CreateUserAsync(User user, string plainPassword);
    }
}
