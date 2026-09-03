using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Entities;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Web.Controllers
{
    [Authorize]
    public class IdeaController : Controller
    {
        private readonly IIdeaService _ideaService;
        private readonly IUserService _userService;

        public IdeaController(IIdeaService ideaService, IUserService userService)
        {
            _ideaService = ideaService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _ideaService.GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string intendedBenefit, string description, int categoryId, List<IFormFile> files, string submitAction)
        {
            var categories = await _ideaService.GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(intendedBenefit) || string.IsNullOrEmpty(description) || categoryId <= 0)
            {
                ViewBag.ErrorMessage = "Lütfen zorunlu tüm alanları doldurun.";
                return View();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            bool isDraft = submitAction == "draft";

            var newIdea = new Idea
            {
                Title = title,
                IntendedBenefit = intendedBenefit,
                Description = description,
                CategoryId = categoryId,
                UserId = userId,
                Status = isDraft ? IdeaStatus.Draft : IdeaStatus.Pending
            };

            var documents = new List<IdeaDocument>();

            if (files != null && files.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ideas");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var document = new IdeaDocument
                        {
                            FileName = file.FileName,
                            FilePath = "/uploads/ideas/" + uniqueFileName,
                            FileExtension = Path.GetExtension(file.FileName),
                            FileSizeBytes = file.Length
                        };

                        documents.Add(document);
                    }
                }
            }

            bool isSuccess = await _ideaService.CreateIdeaAsync(newIdea, documents, isDraft);

            if (isSuccess)
            {
                if (isDraft)
                {
                    TempData["SuccessMessage"] = "Fikriniz taslak olarak kaydedildi. Dilediğiniz zaman düzenleyip havuza atabilirsiniz.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Fikriniz başarıyla havuza atıldı! 🚀";
                }
                return RedirectToAction("MyIdeas");
            }

            ViewBag.ErrorMessage = "Fikir kaydedilirken bir hata oluştu.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            if (idea.UserId != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (idea.Status != IdeaStatus.Pending && idea.Status != IdeaStatus.Draft)
            {
                TempData["ErrorMessage"] = "Sadece taslak veya değerlendirme aşamasındaki fikirlerinizi düzenleyebilirsiniz.";
                return RedirectToAction("MyIdeas");
            }

            var categories = await _ideaService.GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", idea.CategoryId);

            return View(idea);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string intendedBenefit, string description, int categoryId, List<int>? deleteDocumentIds, List<IFormFile>? newFiles)
        {
            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            if (idea.UserId != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (idea.Status != IdeaStatus.Pending && idea.Status != IdeaStatus.Draft)
            {
                TempData["ErrorMessage"] = "Değerlendirilmiş veya geri çekilmiş fikirler düzenlenemez.";
                return RedirectToAction("MyIdeas");
            }

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(intendedBenefit) || string.IsNullOrEmpty(description) || categoryId <= 0)
            {
                var categories = await _ideaService.GetCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
                ViewBag.ErrorMessage = "Lütfen zorunlu tüm alanları doldurun.";
                return View(idea);
            }

            bool isSuccess = await _ideaService.UpdateIdeaAsync(idea, title, intendedBenefit, description, categoryId, currentUserId);

            var newDocuments = new List<IdeaDocument>();
            if (newFiles != null && newFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ideas");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in newFiles)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        newDocuments.Add(new IdeaDocument
                        {
                            FileName = file.FileName,
                            FilePath = "/uploads/ideas/" + uniqueFileName,
                            FileExtension = Path.GetExtension(file.FileName),
                            FileSizeBytes = file.Length
                        });
                    }
                }
            }

            await _ideaService.UpdateIdeaDocumentsAsync(id, deleteDocumentIds, newDocuments, currentUserId);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Fikriniz ve ekli belgeler başarıyla güncellendi.";
                return RedirectToAction("MyIdeas");
            }

            ViewBag.ErrorMessage = "Güncelleme sırasında bir hata oluştu.";
            return View(idea);
        }

        [HttpPost]
        public async Task<IActionResult> PublishDraft(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isSuccess = await _ideaService.PublishDraftIdeaAsync(id, currentUserId);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Fikriniz başarıyla havuza atıldı! 🚀";
            }
            else
            {
                TempData["ErrorMessage"] = "Fikir havuza atılırken bir hata oluştu.";
            }

            return RedirectToAction("MyIdeas");
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isSuccess = await _ideaService.WithdrawIdeaAsync(id, currentUserId);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Fikriniz başarıyla geri çekildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Fikir geri çekilemedi. Sadece taslak veya değerlendirme aşamasındaki fikirlerinizi geri çekebilirsiniz.";
            }

            return RedirectToAction("MyIdeas");
        }

        [HttpGet]
        public async Task<IActionResult> List(string filter = "all", string sort = "date_desc")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.IsEvaluator = isEvaluator;
            ViewBag.IsSuperAdmin = isSuperAdmin;
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentSort = sort;

            var ideas = await _ideaService.GetAllIdeasAsync();

            if (filter == "pending")
            {
                ideas = ideas.Where(i => i.Status == IdeaStatus.Pending);
            }
            else if (filter == "approved")
            {
                ideas = ideas.Where(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
            }
            else if (filter == "rejected")
            {
                ideas = ideas.Where(i => i.Status == IdeaStatus.Rejected);
            }

            ideas = ApplySorting(ideas, sort);

            return View(ideas);
        }

        [HttpGet]
        public async Task<IActionResult> Withdrawn(string sort = "date_desc")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.CurrentSort = sort;
            var withdrawnIdeas = await _ideaService.GetWithdrawnIdeasAsync();
            withdrawnIdeas = ApplySorting(withdrawnIdeas, sort);

            return View(withdrawnIdeas);
        }

        [HttpGet]
        public async Task<IActionResult> MyIdeas(string filter = "all", string sort = "date_desc")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentSort = sort;

            var myIdeas = await _ideaService.GetIdeasByUserIdAsync(currentUserId);

            if (filter == "draft")
            {
                myIdeas = myIdeas.Where(i => i.Status == IdeaStatus.Draft);
            }
            else if (filter == "pending")
            {
                myIdeas = myIdeas.Where(i => i.Status == IdeaStatus.Pending);
            }
            else if (filter == "approved")
            {
                myIdeas = myIdeas.Where(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
            }
            else if (filter == "rejected")
            {
                myIdeas = myIdeas.Where(i => i.Status == IdeaStatus.Rejected);
            }

            myIdeas = ApplySorting(myIdeas, sort);

            return View(myIdeas);
        }

        [HttpGet]
        public async Task<IActionResult> Showcase(int? categoryId = null, string? search = null, string sort = "date_desc")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int currentUserId = userIdClaim != null ? int.Parse(userIdClaim) : 0;

            bool isEvaluator = currentUserId > 0 && await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = currentUserId > 0 && await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            ViewBag.CanManageShowcase = isEvaluator || isSuperAdmin;
            ViewBag.Categories = await _ideaService.GetCategoriesAsync();
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            var implementedIdeas = await _ideaService.GetImplementedIdeasAsync();
            ViewBag.AllImplementedIdeas = implementedIdeas;

            var filteredIdeas = implementedIdeas.AsEnumerable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                filteredIdeas = filteredIdeas.Where(i => i.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.Trim().ToLower();
                filteredIdeas = filteredIdeas.Where(i => 
                    (i.Title != null && i.Title.ToLower().Contains(s)) ||
                    (i.Description != null && i.Description.ToLower().Contains(s)) ||
                    (i.IntendedBenefit != null && i.IntendedBenefit.ToLower().Contains(s)) ||
                    (i.User != null && $"{i.User.FirstName} {i.User.LastName}".ToLower().Contains(s))
                );
            }

            filteredIdeas = ApplySorting(filteredIdeas, sort);

            return View(filteredIdeas);
        }

        [HttpGet]
        public async Task<IActionResult> CreateShowcase()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var allIdeas = await _ideaService.GetAllIdeasAsync();
            var approvedIdeas = allIdeas.Where(i => i.Status == IdeaStatus.Approved && i.UserId != currentUserId).ToList();

            return View(approvedIdeas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShowcase(int referenceIdeaId, string projectTitle, string developmentStory, string achievedBenefit)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (referenceIdeaId <= 0 || string.IsNullOrEmpty(projectTitle) || string.IsNullOrEmpty(developmentStory) || string.IsNullOrEmpty(achievedBenefit))
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları eksiksiz doldurun.";
                return RedirectToAction("CreateShowcase");
            }

            var originalIdea = await _ideaService.GetIdeaByIdAsync(referenceIdeaId);
            if (originalIdea == null) return NotFound();

            if (originalIdea.UserId == currentUserId && !isSuperAdmin)
            {
                TempData["ErrorMessage"] = "Kendi fikrinizi vitrine ekleyemezsiniz.";
                return RedirectToAction("CreateShowcase");
            }

            bool isSuccess = await _ideaService.SetIdeaImplementedAsync(referenceIdeaId);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = $"'{projectTitle}' başarıyla Pırıltılı Fikirler Vitrini'ne eklendi! 🚀";
                return RedirectToAction("Showcase");
            }

            TempData["ErrorMessage"] = "Vitrine eklenirken bir hata oluştu.";
            return RedirectToAction("CreateShowcase");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var idea = await _ideaService.GetIdeaByIdAsync(id);
            if (idea == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");
            bool isAuthor = idea.UserId == currentUserId;

            bool hideAuthor = idea.Status == IdeaStatus.Pending && !isSuperAdmin && !isAuthor;

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.IsEvaluator = isEvaluator || isSuperAdmin;
            ViewBag.IsSuperAdmin = isSuperAdmin;
            ViewBag.IsAuthor = isAuthor;
            ViewBag.HideAuthor = hideAuthor;

            return View(idea);
        }

        [HttpPost]
        public async Task<IActionResult> Evaluate(int ideaId, int score, string comment, string decision)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var idea = await _ideaService.GetIdeaByIdAsync(ideaId);
            if (idea == null) return NotFound();

            if (idea.UserId == currentUserId && !isSuperAdmin)
            {
                TempData["ErrorMessage"] = "Bu fikir size ait. Kendi fikrinizi puanlayamazsınız.";
                return RedirectToAction("Details", new { id = ideaId });
            }

            if (score < 0 || score > 100 || string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(decision))
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir puan (0 ile 100 arasında) ve değerlendirme açıklaması girin.";
                return RedirectToAction("Details", new { id = ideaId });
            }

            if (score >= 50 && decision == "Negative")
            {
                TempData["ErrorMessage"] = "50 ve üzeri puan verilen fikirler olumsuz olarak değerlendirilemez.";
                return RedirectToAction("Details", new { id = ideaId });
            }

            if (score < 50 && decision == "Positive")
            {
                TempData["ErrorMessage"] = "50 puanın altındaki fikirler olumlu olarak değerlendirilemez (Asgari baraj puanı: 50).";
                return RedirectToAction("Details", new { id = ideaId });
            }

            var evaluation = new Evaluation
            {
                IdeaId = ideaId,
                EvaluatorUserId = currentUserId,
                Score = score,
                Comment = comment,
                Decision = decision == "Positive" ? EvaluationDecision.Positive : EvaluationDecision.Negative,
                Status = EvaluationStatus.Approved,
                ApprovedAt = DateTime.Now
            };

            bool isSuccess = await _ideaService.EvaluateIdeaAsync(evaluation);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Fikir başarıyla değerlendirildi ve puanlandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Değerlendirme kaydedilirken bir hata oluştu.";
            }

            return RedirectToAction("Details", new { id = ideaId });
        }

        [HttpPost]
        public async Task<IActionResult> Reopen(int ideaId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");
            if (!isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            bool isSuccess = await _ideaService.ReopenIdeaAsync(ideaId, currentUserId);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Fikir başarıyla yeniden değerlendirme sürecine alındı. 🔄";
            }
            else
            {
                TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu.";
            }

            return RedirectToAction("Details", new { id = ideaId });
        }

        [HttpPost]
        public async Task<IActionResult> Implement(int ideaId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdClaim);

            bool isEvaluator = await _userService.HasPermissionAsync(currentUserId, "IdeaEvaluation");
            bool isSuperAdmin = await _userService.HasPermissionAsync(currentUserId, "PermissionManagement");

            if (!isEvaluator && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var idea = await _ideaService.GetIdeaByIdAsync(ideaId);
            if (idea == null) return NotFound();

            if (idea.UserId == currentUserId && !isSuperAdmin)
            {
                TempData["ErrorMessage"] = "Kendi fikrinizi vitrine ekleyemezsiniz.";
                return RedirectToAction("Details", new { id = ideaId });
            }

            bool isSuccess = await _ideaService.SetIdeaImplementedAsync(ideaId);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Fikir başarıyla hayata geçirildi ve Pırıltılı Fikirler Vitrini'ne eklendi! 🚀";
                return RedirectToAction("Showcase");
            }

            TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu.";
            return RedirectToAction("Details", new { id = ideaId });
        }

        private IEnumerable<Idea> ApplySorting(IEnumerable<Idea> ideas, string sort)
        {
            return sort switch
            {
                "date_asc" => ideas.OrderBy(i => i.CreatedAt),
                "score_desc" => ideas.OrderByDescending(i => i.Evaluations.Any() ? i.Evaluations.Max(e => e.Score) : -1),
                "score_asc" => ideas.OrderBy(i => i.Evaluations.Any() ? i.Evaluations.Min(e => e.Score) : 999),
                "title_asc" => ideas.OrderBy(i => i.Title),
                "title_desc" => ideas.OrderByDescending(i => i.Title),
                _ => ideas.OrderByDescending(i => i.CreatedAt)
            };
        }
    }
}
