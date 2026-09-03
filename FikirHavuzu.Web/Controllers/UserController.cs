using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Entities;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IIdeaService _ideaService;

        public UserController(IUserService userService, IIdeaService ideaService)
        {
            _userService = userService;
            _ideaService = ideaService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.NextRegistrationNumber = await _userService.GenerateUniqueRegistrationNumberAsync(DateTime.UtcNow);
            return View(new User());
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || 
                string.IsNullOrEmpty(user.TCNo) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PhoneNumber))
            {
                ViewBag.NextRegistrationNumber = await _userService.GenerateUniqueRegistrationNumberAsync(DateTime.UtcNow);
                ViewBag.ErrorMessage = "Lütfen tüm zorunlu alanları doldurun.";
                return View(user);
            }

            user.RegistrationNumber = await _userService.GenerateUniqueRegistrationNumberAsync(DateTime.UtcNow);
            
            string tempPassword = FikirHavuzu.Business.Utilities.PasswordGenerator.GenerateTemporaryPassword(10);
            
            bool isCreated = await _userService.CreateUserAsync(user, tempPassword);

            if (isCreated)
            {
                var emailService = HttpContext.RequestServices.GetService(typeof(IEmailService)) as IEmailService;
                if (emailService != null)
                {
                    string loginLink = Url.Action("Login", "Account", null, Request.Scheme) ?? "";
                    await emailService.SendWelcomeCredentialsAsync(user.Email, $"{user.FirstName} {user.LastName}", user.RegistrationNumber, tempPassword, loginLink);
                }

                TempData["SuccessMessage"] = $"{user.FirstName} {user.LastName} adlı personel başarıyla eklendi. Şifre ve Sicil No e-posta ile gönderildi.";
                return RedirectToAction("List");
            }

            ViewBag.NextRegistrationNumber = user.RegistrationNumber;
            ViewBag.ErrorMessage = "Bu T.C. Kimlik No veya E-posta adresi ile zaten kayıtlı bir personel bulunuyor.";
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = (await _userService.GetAllUsersAsync()).ToList();
            var allPermissions = await _userService.GetAllPermissionsAsync();
            var allIdeas = (await _ideaService.GetAllIdeasAsync()).ToList();

            var userPermissionMap = new Dictionary<int, List<int>>();
            var userIdeaStats = new Dictionary<int, (int Total, int Approved, int Implemented, int Points)>();

            foreach (var user in users)
            {
                var permIds = await _userService.GetUserPermissionIdsAsync(user.Id);
                userPermissionMap[user.Id] = permIds;

                var userIdeas = allIdeas.Where(i => i.UserId == user.Id).ToList();
                int total = userIdeas.Count;
                int approved = userIdeas.Count(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
                int implemented = userIdeas.Count(i => i.Status == IdeaStatus.Implemented);
                int points = (approved * 50) + (implemented * 100);

                userIdeaStats[user.Id] = (total, approved, implemented, points);
            }

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.UserPermissionMap = userPermissionMap;
            ViewBag.Permissions = allPermissions;
            ViewBag.IsSuperAdmin = isSuperAdmin;
            ViewBag.UserIdeaStats = userIdeaStats;
            ViewBag.ActiveContributorsCount = userIdeaStats.Count(x => x.Value.Total > 0);
            ViewBag.ApprovedContributorsCount = userIdeaStats.Count(x => x.Value.Approved > 0);
            ViewBag.TotalOrganizationPoints = userIdeaStats.Sum(x => x.Value.Points);

            return View(users);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var userPermIds = await _userService.GetUserPermissionIdsAsync(id);
            var allPermissions = await _userService.GetAllPermissionsAsync();

            int permMgmtPermId = allPermissions.FirstOrDefault(p => p.Name.Contains("PermissionManagement") || p.Name.Contains("Yetki"))?.Id ?? 3;
            bool isTargetSuperAdmin = userPermIds.Contains(permMgmtPermId);

            if (isTargetSuperAdmin && !isSuperAdmin)
            {
                TempData["ErrorMessage"] = "Sistem Yöneticisi hesapları sadece başka bir Sistem Yöneticisi tarafından düzenlenebilir.";
                return RedirectToAction("List");
            }

            // Calculate user idea and innovation stats
            var userIdeas = (await _ideaService.GetAllIdeasAsync()).Where(i => i.UserId == id).ToList();
            int totalIdeas = userIdeas.Count;
            int approvedIdeas = userIdeas.Count(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
            int implementedIdeas = userIdeas.Count(i => i.Status == IdeaStatus.Implemented);
            int userPoints = (approvedIdeas * 50) + (implementedIdeas * 100);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.TotalIdeas = totalIdeas;
            ViewBag.ApprovedIdeas = approvedIdeas;
            ViewBag.ImplementedIdeas = implementedIdeas;
            ViewBag.UserPoints = userPoints;
            ViewBag.UserPermissionIds = userPermIds;
            ViewBag.AllPermissions = allPermissions;
            ViewBag.IsSuperAdmin = isSuperAdmin;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string firstName, string lastName, string email, string tcKimlikNo, string registrationNumber, string phoneNumber, bool isActive, List<int>? permissionIds)
        {
            var currentUserId = GetCurrentUserId();
            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isUserManagement && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(tcKimlikNo) || string.IsNullOrEmpty(registrationNumber))
            {
                ViewBag.ErrorMessage = "Lütfen zorunlu alanları doldurun.";
                var allPerms = await _userService.GetAllPermissionsAsync();
                ViewBag.AllPermissions = allPerms;
                ViewBag.UserPermissionIds = await _userService.GetUserPermissionIdsAsync(id);
                ViewBag.IsSuperAdmin = isSuperAdmin;
                return View(user);
            }

            if (firstName.Any(char.IsDigit) || lastName.Any(char.IsDigit))
            {
                ViewBag.ErrorMessage = "Ad ve Soyad alanlarına rakam girilemez.";
                var allPerms = await _userService.GetAllPermissionsAsync();
                ViewBag.AllPermissions = allPerms;
                ViewBag.UserPermissionIds = await _userService.GetUserPermissionIdsAsync(id);
                ViewBag.IsSuperAdmin = isSuperAdmin;
                return View(user);
            }

            if (tcKimlikNo.Length != 11 || !tcKimlikNo.All(char.IsDigit))
            {
                ViewBag.ErrorMessage = "TC Kimlik Numarası 11 haneli rakamlardan oluşmalıdır.";
                var allPerms = await _userService.GetAllPermissionsAsync();
                ViewBag.AllPermissions = allPerms;
                ViewBag.UserPermissionIds = await _userService.GetUserPermissionIdsAsync(id);
                ViewBag.IsSuperAdmin = isSuperAdmin;
                return View(user);
            }

            var allUsers = await _userService.GetAllUsersAsync();
            if (allUsers.Any(u => u.Id != id && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.ErrorMessage = "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.";
                var allPerms = await _userService.GetAllPermissionsAsync();
                ViewBag.AllPermissions = allPerms;
                ViewBag.UserPermissionIds = await _userService.GetUserPermissionIdsAsync(id);
                ViewBag.IsSuperAdmin = isSuperAdmin;
                return View(user);
            }

            if (allUsers.Any(u => u.Id != id && u.TCNo == tcKimlikNo))
            {
                ViewBag.ErrorMessage = "Bu TC Kimlik Numarası başka bir kullanıcı tarafından kullanılıyor.";
                var allPerms = await _userService.GetAllPermissionsAsync();
                ViewBag.AllPermissions = allPerms;
                ViewBag.UserPermissionIds = await _userService.GetUserPermissionIdsAsync(id);
                ViewBag.IsSuperAdmin = isSuperAdmin;
                return View(user);
            }

            if (allUsers.Any(u => u.Id != id && u.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.ErrorMessage = "Bu Sicil Numarası başka bir kullanıcı tarafından kullanılıyor.";
                var allPerms = await _userService.GetAllPermissionsAsync();
                ViewBag.AllPermissions = allPerms;
                ViewBag.UserPermissionIds = await _userService.GetUserPermissionIdsAsync(id);
                ViewBag.IsSuperAdmin = isSuperAdmin;
                return View(user);
            }

            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.Email = email.Trim();
            user.TCNo = tcKimlikNo.Trim();
            user.RegistrationNumber = registrationNumber.Trim();
            user.PhoneNumber = phoneNumber?.Trim();
            user.IsActive = isActive;

            await _userService.UpdateUserAsync(user);

            if (isSuperAdmin && permissionIds != null)
            {
                await _userService.UpdateUserPermissionsAsync(id, permissionIds, currentUserId);
            }

            TempData["SuccessMessage"] = $"'{user.FirstName} {user.LastName}' adlı çalışanın bilgileri başarıyla güncellendi.";
            return RedirectToAction("List");
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : 0;
        }
    }
}
