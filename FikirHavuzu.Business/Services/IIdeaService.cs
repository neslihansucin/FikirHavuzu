using FikirHavuzu.Entity.Entities;

namespace FikirHavuzu.Business.Services
{
    public interface IIdeaService
    {
        Task<bool> CreateIdeaAsync(Idea idea, List<IdeaDocument> documents, bool isDraft = false);
        Task<bool> UpdateIdeaAsync(Idea idea, string title, string intendedBenefit, string description, int categoryId, int editedByUserId);
        Task<bool> UpdateIdeaDocumentsAsync(int ideaId, List<int>? deleteDocumentIds, List<IdeaDocument>? newDocuments, int editedByUserId);
        Task<Idea?> GetIdeaByIdAsync(int id);
        Task<IEnumerable<Idea>> GetActiveIdeasAsync();
        Task<IEnumerable<Idea>> GetAllIdeasAsync();
        Task<IEnumerable<Idea>> GetIdeasByUserIdAsync(int userId);
        Task<IEnumerable<Idea>> GetPendingEvaluationIdeasAsync();
        Task<bool> EvaluateIdeaAsync(Evaluation evaluation);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Idea>> GetImplementedIdeasAsync();
        Task<bool> SetIdeaImplementedAsync(int ideaId);
        Task<bool> WithdrawIdeaAsync(int ideaId, int userId);
        Task<IEnumerable<Idea>> GetWithdrawnIdeasAsync();
        Task<bool> PublishDraftIdeaAsync(int ideaId, int userId);
        Task<bool> ReopenIdeaAsync(int ideaId, int adminUserId);
    }
}
