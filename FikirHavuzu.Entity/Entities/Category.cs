namespace FikirHavuzu.Entity.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}