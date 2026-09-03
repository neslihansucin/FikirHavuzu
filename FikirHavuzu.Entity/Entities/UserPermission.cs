namespace FikirHavuzu.Entity.Entities;


public class UserPermission : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int PermissionId { get; set; }
    public virtual Permission Permission { get; set; } = null!;

    public int GrantedByUserId { get; set; }
    public virtual User GrantedByUser { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.Now;
}