using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Enums;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FikirHavuzu.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IIdeaService _ideaService;
        private readonly IDistributedCache _cache;

        public DashboardController(IUserService userService, IIdeaService ideaService, IDistributedCache cache)
        {
            _userService = userService;
            _ideaService = ideaService;
            _cache = cache;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int currentUserId = GetCurrentUserId();
            string cacheKey = $"DashboardData_User_{currentUserId}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return Content(cachedData, "application/json");
            }

            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isIdeaEvaluation = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isPermissionManagement = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            bool isManagementUser = isUserManagement || isIdeaEvaluation || isPermissionManagement;
            object dashboardData;

            if (isManagementUser)
            {
                int? totalUsers = null;
                int? totalIdeas = null;
                int? pendingIdeasCount = null;

                if (isUserManagement)
                {
                    var allUsers = await _userService.GetAllUsersAsync();
                    totalUsers = allUsers.Count();
                }

                if (isIdeaEvaluation)
                {
                    var allIdeas = await _ideaService.GetAllIdeasAsync();
                    var pendingIdeas = await _ideaService.GetPendingEvaluationIdeasAsync();
                    totalIdeas = allIdeas.Count();
                    pendingIdeasCount = pendingIdeas.Count();
                }

                dashboardData = new
                {
                    isManagementUser = true,
                    isUserManagement,
                    isIdeaEvaluation,
                    isPermissionManagement,
                    totalUsers,
                    totalIdeas,
                    pendingIdeasCount
                };
            }
            else
            {
                var myIdeas = await _ideaService.GetIdeasByUserIdAsync(currentUserId);

                dashboardData = new
                {
                    isManagementUser = false,
                    isUserManagement = false,
                    isIdeaEvaluation = false,
                    isPermissionManagement = false,
                    myTotalIdeas = myIdeas.Count(),
                    myApprovedIdeas = myIdeas.Count(i => i.Status == IdeaStatus.Approved),
                    myPendingIdeas = myIdeas.Count(i => i.Status == IdeaStatus.Pending || i.Status == IdeaStatus.UnderReview)
                };
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dashboardData), cacheOptions);
            
            return Ok(dashboardData);
        }
    }
}
