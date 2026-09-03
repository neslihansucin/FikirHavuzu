using FikirHavuzu.DataAccess.UnitOfWork;
using FikirHavuzu.Entity.Entities;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Business.Services
{
    public class IdeaService : IIdeaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IdeaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateIdeaAsync(Idea idea, List<IdeaDocument> documents, bool isDraft = false)
        {
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var docRepo = _unitOfWork.GetRepository<IdeaDocument>();

            await ideaRepo.AddAsync(idea);
            int result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0) return false;

            if (documents != null && documents.Any())
            {
                foreach (var doc in documents)
                {
                    doc.IdeaId = idea.Id;
                    await docRepo.AddAsync(doc);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpdateIdeaAsync(Idea idea, string title, string intendedBenefit, string description, int categoryId, int editedByUserId)
        {
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var historyRepo = _unitOfWork.GetRepository<IdeaEditHistory>();

            var oldIdea = await ideaRepo.GetByIdAsync(idea.Id);
            if (oldIdea == null) return false;

            if (oldIdea.Title != title)
            {
                await historyRepo.AddAsync(new IdeaEditHistory
                {
                    IdeaId = idea.Id,
                    UserId = editedByUserId,
                    FieldName = "Fikir Başlığı",
                    OldValue = oldIdea.Title,
                    NewValue = title
                });
                oldIdea.Title = title;
            }

            if (oldIdea.IntendedBenefit != intendedBenefit)
            {
                await historyRepo.AddAsync(new IdeaEditHistory
                {
                    IdeaId = idea.Id,
                    UserId = editedByUserId,
                    FieldName = "Hedeflenen Fayda",
                    OldValue = oldIdea.IntendedBenefit,
                    NewValue = intendedBenefit
                });
                oldIdea.IntendedBenefit = intendedBenefit;
            }

            if (oldIdea.Description != description)
            {
                await historyRepo.AddAsync(new IdeaEditHistory
                {
                    IdeaId = idea.Id,
                    UserId = editedByUserId,
                    FieldName = "Detaylı Açıklama",
                    OldValue = oldIdea.Description,
                    NewValue = description
                });
                oldIdea.Description = description;
            }

            if (oldIdea.CategoryId != categoryId)
            {
                await historyRepo.AddAsync(new IdeaEditHistory
                {
                    IdeaId = idea.Id,
                    UserId = editedByUserId,
                    FieldName = "Kategori",
                    OldValue = oldIdea.CategoryId.ToString(),
                    NewValue = categoryId.ToString()
                });
                oldIdea.CategoryId = categoryId;
            }

            oldIdea.IsEdited = true; 

            ideaRepo.Update(oldIdea);
            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateIdeaDocumentsAsync(int ideaId, List<int>? deleteDocumentIds, List<IdeaDocument>? newDocuments, int editedByUserId)
        {
            var docRepo = _unitOfWork.GetRepository<IdeaDocument>();
            var historyRepo = _unitOfWork.GetRepository<IdeaEditHistory>();
            var ideaRepo = _unitOfWork.GetRepository<Idea>();

            var idea = await ideaRepo.GetByIdAsync(ideaId);
            if (idea == null) return false;

            bool hasChanges = false;

            if (deleteDocumentIds != null && deleteDocumentIds.Any())
            {
                var existingDocs = await docRepo.GetAsync(d => d.IdeaId == ideaId && deleteDocumentIds.Contains(d.Id));
                foreach (var doc in existingDocs)
                {
                    await historyRepo.AddAsync(new IdeaEditHistory
                    {
                        IdeaId = ideaId,
                        UserId = editedByUserId,
                        FieldName = "Ekli Belgeler",
                        OldValue = doc.FileName,
                        NewValue = "[Belge Silindi]"
                    });
                    docRepo.Delete(doc);
                    hasChanges = true;
                }
            }

            if (newDocuments != null && newDocuments.Any())
            {
                foreach (var doc in newDocuments)
                {
                    doc.IdeaId = ideaId;
                    await docRepo.AddAsync(doc);
                    await historyRepo.AddAsync(new IdeaEditHistory
                    {
                        IdeaId = ideaId,
                        UserId = editedByUserId,
                        FieldName = "Ekli Belgeler",
                        OldValue = "[Mevcut Belgeler]",
                        NewValue = $"[Yeni Belge Eklendi: {doc.FileName}]"
                    });
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                idea.IsEdited = true;
                ideaRepo.Update(idea);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Idea?> GetIdeaByIdAsync(int id)
        {
            await _unitOfWork.GetRepository<Category>().GetAllAsync();
            await _unitOfWork.GetRepository<User>().GetAllAsync();
            await _unitOfWork.GetRepository<IdeaDocument>().GetAsync(d => d.IdeaId == id);
            await _unitOfWork.GetRepository<Evaluation>().GetAsync(e => e.IdeaId == id);
            await _unitOfWork.GetRepository<IdeaEditHistory>().GetAsync(h => h.IdeaId == id);

            return await _unitOfWork.GetRepository<Idea>().GetByIdAsync(id);
        }

        public async Task<IEnumerable<Idea>> GetActiveIdeasAsync()
        {
            await _unitOfWork.GetRepository<Category>().GetAllAsync();
            await _unitOfWork.GetRepository<User>().GetAllAsync();

            return await _unitOfWork.GetRepository<Idea>().GetAsync(i => i.Status != IdeaStatus.Rejected && i.Status != IdeaStatus.Withdrawn && i.Status != IdeaStatus.Draft);
        }

        public async Task<IEnumerable<Idea>> GetAllIdeasAsync()
        {
            await _unitOfWork.GetRepository<Category>().GetAllAsync();
            await _unitOfWork.GetRepository<User>().GetAllAsync();

            return await _unitOfWork.GetRepository<Idea>().GetAsync(i => i.Status != IdeaStatus.Withdrawn && i.Status != IdeaStatus.Draft);
        }

        public async Task<IEnumerable<Idea>> GetIdeasByUserIdAsync(int userId)
        {
            await _unitOfWork.GetRepository<Category>().GetAllAsync();

            return await _unitOfWork.GetRepository<Idea>().GetAsync(i => i.UserId == userId);
        }

        public async Task<IEnumerable<Idea>> GetPendingEvaluationIdeasAsync()
        {
            return await _unitOfWork.GetRepository<Idea>().GetAsync(i => i.Status == IdeaStatus.Pending);
        }

        public async Task<bool> EvaluateIdeaAsync(Evaluation evaluation)
        {
            var evaluationRepo = _unitOfWork.GetRepository<Evaluation>();
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var notificationRepo = _unitOfWork.GetRepository<Notification>();

            await evaluationRepo.AddAsync(evaluation);

            if (evaluation.Status == EvaluationStatus.Approved)
            {
                var idea = await ideaRepo.GetByIdAsync(evaluation.IdeaId);
                if (idea != null)
                {
                    if (evaluation.Decision == EvaluationDecision.Positive)
                    {
                        idea.Status = IdeaStatus.Approved;
                    }
                    else if (evaluation.Decision == EvaluationDecision.Negative)
                    {
                        idea.Status = IdeaStatus.Rejected;
                    }
                    ideaRepo.Update(idea);

                    await notificationRepo.AddAsync(new Notification
                    {
                        UserId = idea.UserId,
                        IdeaId = idea.Id, 
                        Message = $"'{idea.Title}' başlıklı fikriniz Fikir Koordinatörü tarafından değerlendirildi. Puanınız: {evaluation.Score}",
                        IsRead = false
                    });
                }
            }

            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _unitOfWork.GetRepository<Category>().GetAllAsync();
        }

        public async Task<IEnumerable<Idea>> GetImplementedIdeasAsync()
        {
            await _unitOfWork.GetRepository<Category>().GetAllAsync();
            await _unitOfWork.GetRepository<User>().GetAllAsync();
            await _unitOfWork.GetRepository<Evaluation>().GetAllAsync();

            return await _unitOfWork.GetRepository<Idea>().GetAsync(i => i.Status == IdeaStatus.Implemented);
        }

        public async Task<bool> SetIdeaImplementedAsync(int ideaId)
        {
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var idea = await ideaRepo.GetByIdAsync(ideaId);
            if (idea == null) return false;

            idea.Status = IdeaStatus.Implemented;
            ideaRepo.Update(idea);

            var notificationRepo = _unitOfWork.GetRepository<Notification>();
            await notificationRepo.AddAsync(new Notification
            {
                UserId = idea.UserId,
                IdeaId = idea.Id,
                Message = $"Tebrikler! '{idea.Title}' başlıklı inovasyon fikriniz başarıyla hayata geçirildi ve Pırıltılı Fikirler Vitrini'nde yayınlandı! 🚀",
                IsRead = false
            });

            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> WithdrawIdeaAsync(int ideaId, int userId)
        {
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var idea = await ideaRepo.GetByIdAsync(ideaId);
            if (idea == null) return false;
            if (idea.UserId != userId) return false;
            if (idea.Status != IdeaStatus.Pending && idea.Status != IdeaStatus.Draft) return false;

            idea.Status = IdeaStatus.Withdrawn;
            ideaRepo.Update(idea);

            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Idea>> GetWithdrawnIdeasAsync()
        {
            await _unitOfWork.GetRepository<Category>().GetAllAsync();
            await _unitOfWork.GetRepository<User>().GetAllAsync();

            return await _unitOfWork.GetRepository<Idea>().GetAsync(i => i.Status == IdeaStatus.Withdrawn);
        }

        public async Task<bool> PublishDraftIdeaAsync(int ideaId, int userId)
        {
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var idea = await ideaRepo.GetByIdAsync(ideaId);
            if (idea == null) return false;
            if (idea.UserId != userId) return false;
            if (idea.Status != IdeaStatus.Draft) return false;

            idea.Status = IdeaStatus.Pending;
            ideaRepo.Update(idea);

            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ReopenIdeaAsync(int ideaId, int adminUserId)
        {
            var ideaRepo = _unitOfWork.GetRepository<Idea>();
            var idea = await ideaRepo.GetByIdAsync(ideaId);
            if (idea == null) return false;

            idea.Status = IdeaStatus.Pending;
            ideaRepo.Update(idea);

            var notificationRepo = _unitOfWork.GetRepository<Notification>();
            await notificationRepo.AddAsync(new Notification
            {
                UserId = idea.UserId,
                IdeaId = idea.Id,
                Message = $"'{idea.Title}' başlıklı fikriniz Sistem Yöneticisi tarafından yeniden değerlendirme sürecine alındı. 🔄",
                IsRead = false
            });

            int result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }
    }
}
