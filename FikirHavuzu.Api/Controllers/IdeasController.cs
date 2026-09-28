using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Entities;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaService _ideaService;
        private readonly IUserService _userService;

        public IdeasController(IIdeaService ideaService, IUserService userService)
        {
            _ideaService = ideaService;
            _userService = userService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _ideaService.GetCategoriesAsync();
            return Ok(categories.Select(c => new { c.Id, c.Name }));
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyIdeas([FromQuery] string filter = "all", [FromQuery] string sort = "date_desc")
        {
            int userId = GetCurrentUserId();
            var ideas = await _ideaService.GetIdeasByUserIdAsync(userId);

            ideas = filter switch
            {
                "draft" => ideas.Where(i => i.Status == IdeaStatus.Draft),
                "pending" => ideas.Where(i => i.Status == IdeaStatus.Pending),
                "approved" => ideas.Where(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented),
                "rejected" => ideas.Where(i => i.Status == IdeaStatus.Rejected),
                _ => ideas
            };

            return Ok(MapIdeas(ApplySorting(ideas, sort), userId, false));
        }

        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string filter = "all", [FromQuery] string sort = "date_desc")
        {
            int userId = GetCurrentUserId();
            bool isEvaluator = await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
                return Forbid();

            var ideas = await _ideaService.GetAllIdeasAsync();

            ideas = filter switch
            {
                "pending" => ideas.Where(i => i.Status == IdeaStatus.Pending),
                "approved" => ideas.Where(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented),
                "rejected" => ideas.Where(i => i.Status == IdeaStatus.Rejected),
                _ => ideas
            };

            return Ok(new
            {
                currentUserId = userId,
                isEvaluator,
                isSuperAdmin,
                ideas = MapIdeas(ApplySorting(ideas, sort), userId, isSuperAdmin)
            });
        }

        [HttpGet("withdrawn")]
        public async Task<IActionResult> Withdrawn([FromQuery] string sort = "date_desc")
        {
            int userId = GetCurrentUserId();
            bool isEvaluator = await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
                return Forbid();

            var ideas = await _ideaService.GetWithdrawnIdeasAsync();
            return Ok(MapIdeas(ApplySorting(ideas, sort), userId, isSuperAdmin));
        }

        [HttpGet("showcase")]
        public async Task<IActionResult> Showcase([FromQuery] int? categoryId, [FromQuery] string? search, [FromQuery] string sort = "date_desc")
        {
            int userId = GetCurrentUserId();
            bool isEvaluator = userId > 0 && await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = userId > 0 && await _userService.HasPermissionAsync(userId, "PermissionManagement");

            var implementedIdeas = await _ideaService.GetImplementedIdeasAsync();
            var filtered = implementedIdeas.AsEnumerable();

            if (categoryId.HasValue && categoryId.Value > 0)
                filtered = filtered.Where(i => i.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.Trim().ToLower();
                filtered = filtered.Where(i =>
                    (i.Title?.ToLower().Contains(s) ?? false) ||
                    (i.Description?.ToLower().Contains(s) ?? false) ||
                    (i.User != null && $"{i.User.FirstName} {i.User.LastName}".ToLower().Contains(s)));
            }

            return Ok(new
            {
                canManageShowcase = isEvaluator || isSuperAdmin,
                categories = (await _ideaService.GetCategoriesAsync()).Select(c => new { c.Id, c.Name }),
                ideas = MapIdeas(ApplySorting(filtered, sort), userId, isSuperAdmin)
            });
        }

        [HttpGet("showcase/approved-for-create")]
        public async Task<IActionResult> ApprovedForShowcase()
        {
            int userId = GetCurrentUserId();
            bool isEvaluator = await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
                return Forbid();

            var allIdeas = await _ideaService.GetAllIdeasAsync();
            var approvedIdeas = allIdeas.Where(i => i.Status == IdeaStatus.Approved && i.UserId != userId).ToList();
            return Ok(MapIdeas(approvedIdeas, userId, isSuperAdmin));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            int userId = GetCurrentUserId();
            bool isEvaluator = await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");
            bool isAuthor = idea.UserId == userId;
            bool hideAuthor = idea.Status == IdeaStatus.Pending && !isSuperAdmin && !isAuthor;

            return Ok(new
            {
                id = idea.Id,
                title = idea.Title,
                description = idea.Description,
                intendedBenefit = idea.IntendedBenefit,
                status = idea.Status.ToString(),
                createdAt = idea.CreatedAt,
                categoryId = idea.CategoryId,
                categoryName = idea.Category?.Name,
                author = hideAuthor ? null : (object?)new { idea.User?.FirstName, idea.User?.LastName, idea.User?.RegistrationNumber, idea.User?.ProfilePictureUrl },
                isAuthor,
                isEvaluator = isEvaluator || isSuperAdmin,
                isSuperAdmin,
                canEvaluate = (isEvaluator || isSuperAdmin) && !isAuthor,
                documents = idea.Documents?.Select(d => new { d.Id, d.FileName, d.FilePath }),
                evaluations = (isEvaluator || isSuperAdmin || isAuthor)
                    ? idea.Evaluations?.OrderByDescending(e => e.CreatedAt).Select(e => new
                    {
                        e.Id, e.Score, e.Comment,
                        decision = e.Decision.ToString(),
                        status = e.Status.ToString(),
                        e.ApprovedAt
                    })
                    : null,
                editHistory = idea.EditHistories?.OrderByDescending(h => h.EditedAt).Select(h => new
                {
                    h.FieldName, h.OldValue, h.NewValue, h.EditedAt
                })
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateIdeaRequest request)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.IntendedBenefit) ||
                string.IsNullOrEmpty(request.Description) || request.CategoryId <= 0)
                return BadRequest(new { message = "Lütfen zorunlu tüm alanları doldurun." });

            bool isDraft = request.SubmitAction == "draft";

            var newIdea = new Idea
            {
                Title = request.Title,
                IntendedBenefit = request.IntendedBenefit,
                Description = request.Description,
                CategoryId = request.CategoryId,
                UserId = userId,
                Status = isDraft ? IdeaStatus.Draft : IdeaStatus.Pending
            };

            var documents = await SaveDocumentsAsync(request.Files);
            bool isSuccess = await _ideaService.CreateIdeaAsync(newIdea, documents, isDraft);

            if (!isSuccess)
                return StatusCode(500, new { message = "Fikir kaydedilirken bir hata oluştu." });

            return Ok(new { message = isDraft ? "Fikriniz taslak olarak kaydedildi." : "Fikriniz başarıyla havuza atıldı! 🚀" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] EditIdeaRequest request)
        {
            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            int userId = GetCurrentUserId();
            if (idea.UserId != userId) return Forbid();

            if (idea.Status != IdeaStatus.Pending && idea.Status != IdeaStatus.Draft)
                return BadRequest(new { message = "Sadece taslak veya değerlendirme aşamasındaki fikirler düzenlenebilir." });

            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.IntendedBenefit) ||
                string.IsNullOrEmpty(request.Description) || request.CategoryId <= 0)
                return BadRequest(new { message = "Lütfen zorunlu tüm alanları doldurun." });

            await _ideaService.UpdateIdeaAsync(idea, request.Title, request.IntendedBenefit, request.Description, request.CategoryId, userId);

            var newDocuments = await SaveDocumentsAsync(request.NewFiles);
            await _ideaService.UpdateIdeaDocumentsAsync(id, request.DeleteDocumentIds, newDocuments, userId);

            return Ok(new { message = "Fikriniz başarıyla güncellendi." });
        }

        [HttpPost("{id}/publish")]
        public async Task<IActionResult> Publish(int id)
        {
            int userId = GetCurrentUserId();
            bool isSuccess = await _ideaService.PublishDraftIdeaAsync(id, userId);
            return isSuccess ? Ok(new { message = "Fikriniz başarıyla havuza atıldı! 🚀" })
                             : BadRequest(new { message = "Fikir havuza atılırken bir hata oluştu." });
        }

        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> Withdraw(int id)
        {
            int userId = GetCurrentUserId();
            bool isSuccess = await _ideaService.WithdrawIdeaAsync(id, userId);
            return isSuccess ? Ok(new { message = "Fikriniz başarıyla geri çekildi." })
                             : BadRequest(new { message = "Sadece taslak veya değerlendirme aşamasındaki fikirler geri çekilebilir." });
        }

        [HttpPost("{id}/evaluate")]
        public async Task<IActionResult> Evaluate(int id, [FromBody] EvaluateRequest request)
        {
            int userId = GetCurrentUserId();
            bool isEvaluator = await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
                return Forbid();

            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            if (idea.UserId == userId && !isSuperAdmin)
                return BadRequest(new { message = "Bu fikir size ait. Kendi fikrinizi puanlayamazsınız." });

            if (request.Score < 0 || request.Score > 100 || string.IsNullOrEmpty(request.Comment) || string.IsNullOrEmpty(request.Decision))
                return BadRequest(new { message = "Geçerli bir puan (0-100) ve değerlendirme açıklaması girin." });

            if (request.Score >= 50 && request.Decision == "Negative")
                return BadRequest(new { message = "50 ve üzeri puan verilen fikirler olumsuz değerlendirilemez." });

            if (request.Score < 50 && request.Decision == "Positive")
                return BadRequest(new { message = "50 puanın altındaki fikirler olumlu değerlendirilemez." });

            var evaluation = new Evaluation
            {
                IdeaId = id,
                EvaluatorUserId = userId,
                Score = request.Score,
                Comment = request.Comment,
                Decision = request.Decision == "Positive" ? EvaluationDecision.Positive : EvaluationDecision.Negative,
                Status = EvaluationStatus.Approved,
                ApprovedAt = DateTime.Now
            };

            bool isSuccess = await _ideaService.EvaluateIdeaAsync(evaluation);
            return isSuccess ? Ok(new { message = "Fikir başarıyla değerlendirildi." })
                             : StatusCode(500, new { message = "Değerlendirme kaydedilirken bir hata oluştu." });
        }

        [HttpPost("{id}/reopen")]
        public async Task<IActionResult> Reopen(int id)
        {
            int userId = GetCurrentUserId();
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");
            if (!isSuperAdmin) return Forbid();

            bool isSuccess = await _ideaService.ReopenIdeaAsync(id, userId);
            return isSuccess ? Ok(new { message = "Fikir yeniden değerlendirme sürecine alındı. 🔄" })
                             : StatusCode(500, new { message = "İşlem sırasında bir hata oluştu." });
        }

        [HttpPost("{id}/implement")]
        public async Task<IActionResult> Implement(int id)
        {
            int userId = GetCurrentUserId();
            bool isEvaluator = await _userService.HasPermissionAsync(userId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(userId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin) return Forbid();

            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            if (idea.UserId == userId && !isSuperAdmin)
                return BadRequest(new { message = "Kendi fikrinizi vitrine ekleyemezsiniz." });

            bool isSuccess = await _ideaService.SetIdeaImplementedAsync(id);
            return isSuccess ? Ok(new { message = "Fikir Pırıltılı Fikirler Vitrini'ne eklendi! 🚀" })
                             : StatusCode(500, new { message = "İşlem sırasında bir hata oluştu." });
        }

        private async Task<List<IdeaDocument>> SaveDocumentsAsync(List<IFormFile>? files)
        {
            var documents = new List<IdeaDocument>();
            if (files == null || files.Count == 0) return documents;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ideas");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    documents.Add(new IdeaDocument
                    {
                        FileName = file.FileName,
                        FilePath = "/uploads/ideas/" + uniqueFileName,
                        FileExtension = Path.GetExtension(file.FileName),
                        FileSizeBytes = file.Length
                    });
                }
            }

            return documents;
        }

        private static IEnumerable<Idea> ApplySorting(IEnumerable<Idea> ideas, string sort) => sort switch
        {
            "date_asc" => ideas.OrderBy(i => i.CreatedAt),
            "score_desc" => ideas.OrderByDescending(i => i.Evaluations.Any() ? i.Evaluations.Max(e => e.Score) : -1),
            "score_asc" => ideas.OrderBy(i => i.Evaluations.Any() ? i.Evaluations.Min(e => e.Score) : 999),
            "title_asc" => ideas.OrderBy(i => i.Title),
            "title_desc" => ideas.OrderByDescending(i => i.Title),
            _ => ideas.OrderByDescending(i => i.CreatedAt)
        };

        private static IEnumerable<object> MapIdeas(IEnumerable<Idea> ideas, int currentUserId, bool isSuperAdmin) =>
            ideas.Select(i => new
            {
                i.Id,
                i.Title,
                i.Description,
                i.IntendedBenefit,
                status = i.Status.ToString(),
                i.CreatedAt,
                i.CategoryId,
                categoryName = i.Category?.Name,
                userId = i.UserId,
                authorName = isSuperAdmin || i.Status != IdeaStatus.Pending
                    ? $"{i.User?.FirstName} {i.User?.LastName}"
                    : (i.UserId == currentUserId ? $"{i.User?.FirstName} {i.User?.LastName}" : "🔒 Gizli Katılımcı"),
                authorProfilePictureUrl = isSuperAdmin || i.Status != IdeaStatus.Pending
                    ? i.User?.ProfilePictureUrl
                    : (i.UserId == currentUserId ? i.User?.ProfilePictureUrl : null),
                isOwner = i.UserId == currentUserId,
                latestScore = i.Evaluations?.OrderByDescending(e => e.CreatedAt).FirstOrDefault()?.Score,
                documentsCount = i.Documents?.Count ?? 0
            });
    }

    public class CreateIdeaRequest
    {
        public string Title { get; set; } = string.Empty;
        public string IntendedBenefit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public int CategoryId { get; set; }
        public string SubmitAction { get; set; } = "submit";
        public List<IFormFile>? Files { get; set; }
    }

    public class EditIdeaRequest
    {
        public string Title { get; set; } = string.Empty;
        public string IntendedBenefit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public int CategoryId { get; set; }
        public List<int>? DeleteDocumentIds { get; set; }
        public List<IFormFile>? NewFiles { get; set; }
    }

    public record EvaluateRequest([System.ComponentModel.DataAnnotations.Required] int Score, string Comment, string Decision);
}
