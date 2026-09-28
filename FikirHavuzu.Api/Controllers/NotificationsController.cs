using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.DataAccess.UnitOfWork;
using FikirHavuzu.Entity.Entities;

namespace FikirHavuzu.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            int userId = GetCurrentUserId();
            var repo = _unitOfWork.GetRepository<Notification>();
            var unread = await repo.GetAsync(n => n.UserId == userId && !n.IsRead);

            var result = unread.Select(n => new
            {
                n.Id,
                n.Message,
                n.IdeaId,
                isImplemented = n.Message.Contains("hayata geçirildi") || n.Message.Contains("Vitrin"),
                isEvaluated = n.Message.Contains("değerlendirildi")
            });

            return Ok(new { success = true, data = result });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int userId = GetCurrentUserId();
            var repo = _unitOfWork.GetRepository<Notification>();
            var notification = await repo.GetByIdAsync(id);

            if (notification == null || notification.UserId != userId)
                return NotFound(new { success = false });

            notification.IsRead = true;
            repo.Update(notification);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> Leaderboard()
        {
            int currentUserId = GetCurrentUserId();
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var userRepo = _unitOfWork.GetRepository<FikirHavuzu.Entity.Entities.User>();

            var allIdeas = await ideaRepo.GetAsync(
                i => i.Status == FikirHavuzu.Entity.Enums.IdeaStatus.Approved ||
                     i.Status == FikirHavuzu.Entity.Enums.IdeaStatus.Implemented);

            var allUsers = await userRepo.GetAsync(u => u.IsActive);

            var leaderboard = allIdeas
                .GroupBy(i => i.UserId)
                .Select(g =>
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == g.Key);
                    int approved = g.Count();
                    int implemented = g.Count(x => x.Status == FikirHavuzu.Entity.Enums.IdeaStatus.Implemented);
                    int points = (approved * 50) + (implemented * 100);
                    int juryScore = g.Sum(x => x.Evaluations?.OrderByDescending(e => e.CreatedAt).FirstOrDefault()?.Score ?? 0);

                    return new
                    {
                        userId = g.Key,
                        firstName = user?.FirstName,
                        lastName = user?.LastName,
                        profilePictureUrl = user?.ProfilePictureUrl,
                        approvedCount = approved,
                        implementedCount = implemented,
                        points,
                        juryScoreSum = juryScore,
                        isCurrentUser = g.Key == currentUserId
                    };
                })
                .OrderByDescending(x => x.points)
                .ThenByDescending(x => x.juryScoreSum)
                .ToList();

            return Ok(leaderboard);
        }
    }
}
