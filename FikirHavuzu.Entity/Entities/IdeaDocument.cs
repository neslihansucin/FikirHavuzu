namespace FikirHavuzu.Entity.Entities;

public class IdeaDocument : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int IdeaId { get; set; }
    public virtual Idea Idea { get; set; } = null!;
}