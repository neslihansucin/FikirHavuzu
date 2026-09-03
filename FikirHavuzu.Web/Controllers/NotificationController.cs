using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FikirHavuzu.DataAccess.UnitOfWork;
using FikirHavuzu.Entity.Entities;
using System.Security.Claims;

namespace FikirHavuzu.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnread()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { success = false });

            var repo = _unitOfWork.GetRepository<Notification>();
            var unreadNotifications = await repo.GetAsync(n => n.UserId == userId && !n.IsRead);
            
            var result = unreadNotifications.Select(n => new 
            {
                id = n.Id,
                message = n.Message,
                ideaId = n.IdeaId,
                isImplemented = n.Message.Contains("hayata geçirildi") || n.Message.Contains("Vitrin"),
                isEvaluated = n.Message.Contains("değerlendirildi")
            }).ToList();

            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { success = false });

            var repo = _unitOfWork.GetRepository<Notification>();
            var notification = await repo.GetByIdAsync(id);

            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                repo.Update(notification);
                await _unitOfWork.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
