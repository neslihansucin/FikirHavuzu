using FikirHavuzu.Entity.Enums;


namespace FikirHavuzu.Entity.Entities;

public class Evaluation : BaseEntity
{
    public int IdeaId { get; set; }
    public virtual Idea Idea { get; set; } = null!;

    public int EvaluatorUserId { get; set; }
    public virtual User EvaluatorUser { get; set; } = null!;

    public EvaluationDecision Decision { get; set; }

    public int Score { get; set; }

    public string Comment { get; set; } = string.Empty;

    public EvaluationStatus Status { get; set; } = EvaluationStatus.Draft;

    public DateTime? ApprovedAt { get; set; }
}

