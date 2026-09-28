using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Entities;
using FikirHavuzu.Entity.Enums;
using FikirHavuzu.Business.Utilities;

namespace FikirHavuzu.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IIdeaService _ideaService;
        private readonly IEmailService _emailService;

        public UsersController(IUserService userService, IIdeaService ideaService, IEmailService emailService)
        {
            _userService = userService;
            _ideaService = ideaService;
            _emailService = emailService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        public async Task<IActionResult> List()
        {
            int currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin) return Forbid();

            var users = (await _userService.GetAllUsersAsync()).ToList();
            var allPermissions = await _userService.GetAllPermissionsAsync();
            var allIdeas = (await _ideaService.GetAllIdeasAsync()).ToList();

            var result = new List<object>();
            foreach (var user in users)
            {
                var permIds = await _userService.GetUserPermissionIdsAsync(user.Id);
                var userIdeas = allIdeas.Where(i => i.UserId == user.Id).ToList();
                int approved = userIdeas.Count(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
                int implemented = userIdeas.Count(i => i.Status == IdeaStatus.Implemented);

                result.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.RegistrationNumber,
                    user.PhoneNumber,
                    user.IsActive,
                    user.ProfilePictureUrl,
                    permissions = allPermissions.Where(p => permIds.Contains(p.Id)).Select(p => new { p.Id, p.Name }),
                    stats = new
                    {
                        total = userIdeas.Count,
                        approved,
                        implemented,
                        points = (approved * 50) + (implemented * 100)
                    }
                });
            }

            return Ok(new
            {
                currentUserId,
                isSuperAdmin,
                permissions = allPermissions.Select(p => new { p.Id, p.Name }),
                users = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            int currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin) return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var userPermIds = await _userService.GetUserPermissionIdsAsync(id);
            var allPermissions = await _userService.GetAllPermissionsAsync();
            var userIdeas = (await _ideaService.GetAllIdeasAsync()).Where(i => i.UserId == id).ToList();

            int approved = userIdeas.Count(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
            int implemented = userIdeas.Count(i => i.Status == IdeaStatus.Implemented);

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.TCNo,
                user.RegistrationNumber,
                user.PhoneNumber,
                user.IsActive,
                user.ProfilePictureUrl,
                permissionIds = userPermIds,
                allPermissions = allPermissions.Select(p => new { p.Id, p.Name }),
                isSuperAdmin,
                stats = new { total = userIdeas.Count, approved, implemented, points = (approved * 50) + (implemented * 100) }
            });
        }

        [HttpGet("next-registration-number")]
        public async Task<IActionResult> NextRegistrationNumber()
        {
            int currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin) return Forbid();

            var regNo = await _userService.GenerateUniqueRegistrationNumberAsync(DateTime.UtcNow);
            return Ok(new { registrationNumber = regNo });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            int currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin) return Forbid();

            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) ||
                string.IsNullOrEmpty(request.TCNo) || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.PhoneNumber))
                return BadRequest(new { message = "Lütfen tüm zorunlu alanları doldurun." });

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                TCNo = request.TCNo.Trim(),
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber.Trim()
            };

            user.RegistrationNumber = await _userService.GenerateUniqueRegistrationNumberAsync(DateTime.UtcNow);
            string tempPassword = PasswordGenerator.GenerateTemporaryPassword(10);
            bool isCreated = await _userService.CreateUserAsync(user, tempPassword);

            if (!isCreated)
                return BadRequest(new { message = "Bu T.C. Kimlik No veya E-posta ile zaten kayıtlı bir personel bulunuyor." });

            string loginLink = $"{request.BaseUrl}/auth/login";
            await _emailService.SendWelcomeCredentialsAsync(user.Email, $"{user.FirstName} {user.LastName}", user.RegistrationNumber, tempPassword, loginLink);

            return Ok(new { message = $"{user.FirstName} {user.LastName} adlı personel başarıyla eklendi.", registrationNumber = user.RegistrationNumber });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditUserRequest request)
        {
            int currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin) return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) ||
                string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.TCNo) ||
                string.IsNullOrEmpty(request.RegistrationNumber))
                return BadRequest(new { message = "Lütfen zorunlu alanları doldurun." });

            if (request.FirstName.Any(char.IsDigit) || request.LastName.Any(char.IsDigit))
                return BadRequest(new { message = "Ad ve Soyad alanlarına rakam girilemez." });

            if (request.TCNo.Length != 11 || !request.TCNo.All(char.IsDigit))
                return BadRequest(new { message = "TC Kimlik Numarası 11 haneli rakamlardan oluşmalıdır." });

            var allUsers = await _userService.GetAllUsersAsync();

            if (allUsers.Any(u => u.Id != id && u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { message = "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor." });

            if (allUsers.Any(u => u.Id != id && u.TCNo == request.TCNo))
                return BadRequest(new { message = "Bu TC Kimlik Numarası başka bir kullanıcı tarafından kullanılıyor." });

            if (allUsers.Any(u => u.Id != id && u.RegistrationNumber.Equals(request.RegistrationNumber, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { message = "Bu Sicil Numarası başka bir kullanıcı tarafından kullanılıyor." });

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = request.Email.Trim();
            user.TCNo = request.TCNo.Trim();
            user.RegistrationNumber = request.RegistrationNumber.Trim();
            user.PhoneNumber = request.PhoneNumber?.Trim();
            user.IsActive = request.IsActive;

            await _userService.UpdateUserAsync(user);

            if (isSuperAdmin && request.PermissionIds != null)
                await _userService.UpdateUserPermissionsAsync(id, request.PermissionIds, currentUserId);

            return Ok(new { message = $"'{user.FirstName} {user.LastName}' adlı çalışanın bilgileri başarıyla güncellendi." });
        }
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TCNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class EditUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TCNo { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public bool IsActive { get; set; }
        public List<int>? PermissionIds { get; set; }
    }
}
