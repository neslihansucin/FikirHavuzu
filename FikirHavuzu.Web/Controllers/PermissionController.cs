using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;

namespace FikirHavuzu.Web.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly IUserService _userService;

        public PermissionController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool hasPermission = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = await _userService.GetAllUsersAsync();
            var permissions = await _userService.GetAllPermissionsAsync();

            var userPermissionMap = new Dictionary<int, List<int>>();
            foreach (var user in users)
            {
                userPermissionMap[user.Id] = await _userService.GetUserPermissionIdsAsync(user.Id);
            }

            ViewBag.Permissions = permissions;
            ViewBag.UserPermissionMap = userPermissionMap;
            ViewBag.CurrentUserId = currentUserId;

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int targetUserId, List<int> permissionIds)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool hasPermission = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            permissionIds ??= new List<int>();

            if (targetUserId == currentUserId)
            {
                var permissions = await _userService.GetAllPermissionsAsync();
                var pmPerm = permissions.FirstOrDefault(p => p.Name == "PermissionManagement");
                
                if (pmPerm != null && !permissionIds.Contains(pmPerm.Id))
                {
                    TempData["ErrorMessage"] = "Güvenlik İhlali: Kendi yetkileriniz arasından 'Yetki Yönetimi' yetkisini kaldıramazsınız. Aksi halde sistemi yönetemezsiniz!";
                    return RedirectToAction("Manage");
                }
            }

            await _userService.UpdateUserPermissionsAsync(targetUserId, permissionIds, currentUserId);
            TempData["SuccessMessage"] = "Kullanıcı yetkileri başarıyla güncellendi.";

            return RedirectToAction("Manage");
        }
    }
}
