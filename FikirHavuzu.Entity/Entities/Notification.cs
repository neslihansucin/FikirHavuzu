namespace FikirHavuzu.Entity.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int IdeaId { get; set; }
    public virtual Idea Idea { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
}