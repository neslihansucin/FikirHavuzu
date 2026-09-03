using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Entity.Entities;

public class Idea : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string IntendedBenefit { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public IdeaStatus Status { get; set; } = IdeaStatus.Pending;

    public bool IsEdited { get; set; } = false;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<IdeaDocument> Documents { get; set; } = new List<IdeaDocument>();
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    public virtual ICollection<IdeaEditHistory> EditHistories { get; set; } = new List<IdeaEditHistory>();
     public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}