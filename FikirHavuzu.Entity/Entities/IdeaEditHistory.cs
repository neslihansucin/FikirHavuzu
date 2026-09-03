namespace FikirHavuzu.Entity.Entities;


public class IdeaEditHistory : BaseEntity
{
    public int IdeaId { get; set; }
    public virtual Idea Idea { get; set; } = null!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public string FieldName { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; } = DateTime.Now;
}