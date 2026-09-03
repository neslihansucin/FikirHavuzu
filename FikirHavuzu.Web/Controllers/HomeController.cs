using System.Diagnostics;
using FikirHavuzu.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;
        private readonly IIdeaService _ideaService;

        public HomeController(ILogger<HomeController> logger, IUserService userService, IIdeaService ideaService)
        {
            _logger = logger;
            _userService = userService;
            _ideaService = ideaService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isUserManagement = await _userService.HasPermissionAsync(currentUserId, "UserManagement");
            bool isIdeaEvaluation = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isPermissionManagement = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            ViewBag.IsUserManagement = isUserManagement;
            ViewBag.IsIdeaEvaluation = isIdeaEvaluation;
            ViewBag.IsPermissionManagement = isPermissionManagement;

            if (isUserManagement || isIdeaEvaluation || isPermissionManagement)
            {
                ViewBag.IsManagementUser = true;

                if (isUserManagement)
                {
                    var allUsers = await _userService.GetAllUsersAsync();
                    ViewBag.TotalUsers = allUsers.Count();
                }

                if (isIdeaEvaluation)
                {
                    var allIdeas = await _ideaService.GetAllIdeasAsync();
                    var pendingIdeas = await _ideaService.GetPendingEvaluationIdeasAsync();
                    ViewBag.TotalIdeas = allIdeas.Count();
                    ViewBag.PendingIdeasCount = pendingIdeas.Count();
                }
            }
            else
            {
                ViewBag.IsManagementUser = false;
                var myIdeas = await _ideaService.GetIdeasByUserIdAsync(currentUserId);
                
                ViewBag.MyTotalIdeas = myIdeas.Count();
                ViewBag.MyApprovedIdeas = myIdeas.Count(i => i.Status == IdeaStatus.Approved);
                ViewBag.MyPendingIdeas = myIdeas.Count(i => i.Status == IdeaStatus.Pending || i.Status == IdeaStatus.UnderReview);
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
