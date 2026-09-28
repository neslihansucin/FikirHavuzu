using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;

namespace FikirHavuzu.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IUserService _userService;

        public PermissionsController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            int currentUserId = GetCurrentUserId();
            bool hasPermission = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");
            if (!hasPermission) return Forbid();

            var users = await _userService.GetAllUsersAsync();
            var permissions = await _userService.GetAllPermissionsAsync();

            var userPermissionMap = new List<object>();
            foreach (var user in users)
            {
                var permIds = await _userService.GetUserPermissionIdsAsync(user.Id);
                userPermissionMap.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.RegistrationNumber,
                    user.IsActive,
                    permissionIds = permIds
                });
            }

            return Ok(new
            {
                currentUserId,
                permissions = permissions.Select(p => new { p.Id, p.Name }),
                users = userPermissionMap
            });
        }

        [HttpPut("{targetUserId}")]
        public async Task<IActionResult> Update(int targetUserId, [FromBody] UpdatePermissionsRequest request)
        {
            int currentUserId = GetCurrentUserId();
            bool hasPermission = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");
            if (!hasPermission) return Forbid();

            var permissionIds = request.PermissionIds ?? new List<int>();

            if (targetUserId == currentUserId)
            {
                var permissions = await _userService.GetAllPermissionsAsync();
                var pmPerm = permissions.FirstOrDefault(p => p.Name == "PermissionManagement");
                if (pmPerm != null && !permissionIds.Contains(pmPerm.Id))
                    return BadRequest(new { message = "Kendi 'Yetki Yönetimi' yetkisini kaldıramazsınız." });
            }

            await _userService.UpdateUserPermissionsAsync(targetUserId, permissionIds, currentUserId);
            return Ok(new { message = "Kullanıcı yetkileri başarıyla güncellendi." });
        }
    }

    public class UpdatePermissionsRequest
    {
        public List<int>? PermissionIds { get; set; }
    }
}
