using System;
using System.Collections.Generic;

namespace FikirHavuzu.Entity.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }                    
        public string TCNo { get; set; } = string.Empty;           
        public string RegistrationNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string PasswordHash { get; set; } = string.Empty;   
        public string PasswordSalt { get; set; } = string.Empty;    
        public bool IsActive { get; set; } = true;                 
        public bool IsPasswordChangeRequired { get; set; } = true;
        
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }

        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
        public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
