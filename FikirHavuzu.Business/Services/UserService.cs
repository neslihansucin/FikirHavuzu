using FikirHavuzu.Entity.Entities;
using FikirHavuzu.DataAccess.UnitOfWork;
using FikirHavuzu.Business.Utilities;

namespace FikirHavuzu.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> AuthenticateAsync(string registrationNumber, string password)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.FirstOrDefaultAsync(u => u.RegistrationNumber == registrationNumber);
            if (user == null)
                return null;

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordCorrect)
                return null;

            return user;
        }

        public async Task<bool> RegisterUserAsync(User user, string plainPassword)
        {
            var userRepo = _unitOfWork.GetRepository<User>();

            var existingUser = await userRepo.FirstOrDefaultAsync(u => 
                u.Email == user.Email || 
                u.TCNo == user.TCNo || 
                u.RegistrationNumber == user.RegistrationNumber);

            if (existingUser != null)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            user.IsActive = true;
            user.IsPasswordChangeRequired = true;

            await userRepo.AddAsync(user);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<User>().GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.GetRepository<User>().GetAllAsync();
        }

        public async Task<bool> AssignPermissionToUserAsync(int userId, int permissionId, int grantedByUserId)
        {
            var userPermissionRepo = _unitOfWork.GetRepository<UserPermission>();

            var existingPermission = await userPermissionRepo.FirstOrDefaultAsync(up => 
                up.UserId == userId && up.PermissionId == permissionId);

            if (existingPermission != null)
                return true;

            var newUserPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId,
                GrantedByUserId = grantedByUserId
            };

            await userPermissionRepo.AddAsync(newUserPermission);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            var userPermissionRepo = _unitOfWork.GetRepository<UserPermission>();
            var permissionRepo = _unitOfWork.GetRepository<Permission>();

            var userPermissions = await userPermissionRepo.GetAsync(up => up.UserId == userId);
            var userPermissionIds = userPermissions.Select(up => up.PermissionId).ToList();

            var targetPermission = await permissionRepo.FirstOrDefaultAsync(p => p.Name == permissionName);
            if (targetPermission == null) 
                return false;

            return userPermissionIds.Contains(targetPermission.Id);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.IsPasswordChangeRequired = false;

            userRepo.Update(user);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            userRepo.Update(user);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _unitOfWork.GetRepository<Permission>().GetAllAsync();
        }

        public async Task<List<int>> GetUserPermissionIdsAsync(int userId)
        {
            var userPerms = await _unitOfWork.GetRepository<UserPermission>().GetAsync(up => up.UserId == userId);
            return userPerms.Select(up => up.PermissionId).ToList();
        }

        public async Task<bool> UpdateUserPermissionsAsync(int userId, List<int> selectedPermissionIds, int grantedByUserId)
        {
            var userPermRepo = _unitOfWork.GetRepository<UserPermission>();
            var currentPerms = await userPermRepo.GetAsync(up => up.UserId == userId);

            foreach (var perm in currentPerms)
            {
                if (!selectedPermissionIds.Contains(perm.PermissionId))
                {
                    userPermRepo.Delete(perm);
                }
            }

            var currentPermIds = currentPerms.Select(p => p.PermissionId).ToList();
            foreach (var permId in selectedPermissionIds)
            {
                if (!currentPermIds.Contains(permId))
                {
                    await userPermRepo.AddAsync(new UserPermission
                    {
                        UserId = userId,
                        PermissionId = permId,
                        GrantedByUserId = grantedByUserId,
                        GrantedAt = DateTime.Now
                    });
                }
            }

            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }
        public async Task<string> GenerateUniqueRegistrationNumberAsync(DateTime registrationDate)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var allUsers = await userRepo.GetAllAsync();
            var regNumbers = allUsers.Select(u => u.RegistrationNumber).ToList();
            return PasswordGenerator.GenerateRegistrationNumber(regNumbers);
        }

        public async Task<bool> SavePasswordResetTokenAsync(string email, string token, DateTime expiration)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
            if (user == null) return false;

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiration = expiration;
            
            userRepo.Update(user);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<User?> ValidatePasswordResetTokenAsync(string email, string token)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.FirstOrDefaultAsync(u => 
                u.Email.ToLower() == email.Trim().ToLower() && 
                u.PasswordResetToken == token &&
                u.PasswordResetTokenExpiration > DateTime.UtcNow);
                
            return user;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await ValidatePasswordResetTokenAsync(email, token);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            user.IsPasswordChangeRequired = false;

            userRepo.Update(user);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CreateUserAsync(User user, string plainPassword)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            
            var existingUser = await userRepo.FirstOrDefaultAsync(u => 
                u.Email == user.Email || 
                u.TCNo == user.TCNo);

            if (existingUser != null)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            user.IsActive = true;
            user.IsPasswordChangeRequired = true;
            user.PasswordSalt = Guid.NewGuid().ToString();

            await userRepo.AddAsync(user);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }
    }
}
