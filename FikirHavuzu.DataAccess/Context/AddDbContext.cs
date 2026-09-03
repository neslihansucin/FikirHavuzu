using FikirHavuzu.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace FikirHavuzu.DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<UserPermission> UserPermissions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Idea> Ideas { get; set; } = null!;
        public DbSet<IdeaDocument> IdeaDocuments { get; set; } = null!;
        public DbSet<Evaluation> Evaluations { get; set; } = null!;
        public DbSet<IdeaEditHistory> IdeaEditHistories { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.TCNo).IsUnique();
                entity.HasIndex(u => u.RegistrationNumber).IsUnique();

                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
                entity.Property(u => u.TCNo).HasMaxLength(11).IsRequired();
                entity.Property(u => u.RegistrationNumber).HasMaxLength(20).IsRequired();
                entity.Property(u => u.PasswordHash).HasMaxLength(250).IsRequired();
                entity.Property(u => u.PasswordSalt).HasMaxLength(100).IsRequired();
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(p => p.Name).IsUnique();               
                entity.Property(p => p.Name).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(250);
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasOne(up => up.User)
                      .WithMany(u => u.UserPermissions)
                      .HasForeignKey(up => up.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(up => up.Permission)
                      .WithMany(p => p.UserPermissions)
                      .HasForeignKey(up => up.PermissionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(up => up.GrantedByUser)
                      .WithMany()
                      .HasForeignKey(up => up.GrantedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Idea>(entity =>
            {
                entity.Property(i => i.Title).HasMaxLength(150).IsRequired();
                entity.Property(i => i.IntendedBenefit).HasMaxLength(500).IsRequired();
                entity.Property(i => i.Description).IsRequired();

                entity.HasOne(i => i.User)
                      .WithMany(u => u.Ideas)
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Category)
                      .WithMany(c => c.Ideas)
                      .HasForeignKey(i => i.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Evaluation>(entity =>
            {
                entity.Property(e => e.Comment).HasMaxLength(1000);

                entity.HasOne(e => e.Idea)
                      .WithMany(i => i.Evaluations)
                      .HasForeignKey(e => e.IdeaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.EvaluatorUser)
                      .WithMany(u => u.Evaluations)
                      .HasForeignKey(e => e.EvaluatorUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IdeaEditHistory>(entity =>
            {
                entity.HasOne(h => h.Idea)
                      .WithMany(i => i.EditHistories)
                      .HasForeignKey(h => h.IdeaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.User)
                      .WithMany()
                      .HasForeignKey(h => h.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Message).HasMaxLength(250).IsRequired();

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Idea)
                      .WithMany(i => i.Notifications)
                      .HasForeignKey(n => n.IdeaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "UserManagement", Description = "Kullanıcı ekleme, güncelleme ve pasife alma yetkisi", CreatedAt = new DateTime(2026, 1, 1) },
                new Permission { Id = 2, Name = "IdeaEvaluation", Description = "Fikir ve önerileri değerlendirme ve puanlama yetkisi", CreatedAt = new DateTime(2026, 1, 1) },
                new Permission { Id = 3, Name = "PermissionManagement", Description = "Kullanıcılara sistem yetkilerini atama ve kaldırma yetkisi", CreatedAt = new DateTime(2026, 1, 1) }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Ürün İyileştirme", Description = "Mevcut veya yeni ürünlere yönelik geliştirme fikirleri", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Category { Id = 2, Name = "Hizmet Geliştirme", Description = "Müşteri ve çalışan hizmet kalitesini artırıcı öneriler", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Category { Id = 3, Name = "Süreç & Verimlilik", Description = "Şirket içi operasyonel süreçleri hızlandıran inovatif fikirler", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Category { Id = 4, Name = "Diğer", Description = "Diğer kategorilere uymayan her türlü inovatif fikir", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",
                    Email = "admin@fikirhavuzu.com",
                    TCNo = "11111111111",
                    RegistrationNumber = "adm001",
                    PhoneNumber = "05550000000",
                    PasswordHash = "$2a$11$uFp.3fQZzX7rGqF0Gq9l5e1ZKp3gN8sW8hXJ8yD7gL9kP1mQ2r3tS", 
                    PasswordSalt = "STATIC_ADMIN_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            modelBuilder.Entity<UserPermission>().HasData(
                new UserPermission { Id = 1, UserId = 1, PermissionId = 1, GrantedByUserId = 1, GrantedAt = new DateTime(2026, 1, 1), CreatedAt = new DateTime(2026, 1, 1) },
                new UserPermission { Id = 2, UserId = 1, PermissionId = 2, GrantedByUserId = 1, GrantedAt = new DateTime(2026, 1, 1), CreatedAt = new DateTime(2026, 1, 1) },
                new UserPermission { Id = 3, UserId = 1, PermissionId = 3, GrantedByUserId = 1, GrantedAt = new DateTime(2026, 1, 1), CreatedAt = new DateTime(2026, 1, 1) }
            );
        }
    }
}
