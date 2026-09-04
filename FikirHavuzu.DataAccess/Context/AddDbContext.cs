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
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", 
                    PasswordSalt = "STATIC_ADMIN_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 101,
                    FirstName = "Mehmet",
                    LastName = "Yılmaz",
                    Email = "mehmet.yilmaz@fikirhavuzu.com",
                    TCNo = "22222222221",
                    RegistrationNumber = "PER260901001",
                    PhoneNumber = "05320000001",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 102,
                    FirstName = "Ayşe",
                    LastName = "Kaya",
                    Email = "ayse.kaya@fikirhavuzu.com",
                    TCNo = "22222222222",
                    RegistrationNumber = "PER260901002",
                    PhoneNumber = "05320000002",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 103,
                    FirstName = "Ali",
                    LastName = "Demir",
                    Email = "ali.demir@fikirhavuzu.com",
                    TCNo = "22222222223",
                    RegistrationNumber = "PER260901003",
                    PhoneNumber = "05320000003",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 104,
                    FirstName = "Zeynep",
                    LastName = "Çelik",
                    Email = "zeynep.celik@fikirhavuzu.com",
                    TCNo = "22222222224",
                    RegistrationNumber = "PER260901004",
                    PhoneNumber = "05320000004",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 105,
                    FirstName = "Mustafa",
                    LastName = "Şahin",
                    Email = "mustafa.sahin@fikirhavuzu.com",
                    TCNo = "22222222225",
                    RegistrationNumber = "PER260901005",
                    PhoneNumber = "05320000005",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 106,
                    FirstName = "Elif",
                    LastName = "Öztürk",
                    Email = "elif.ozturk@fikirhavuzu.com",
                    TCNo = "22222222226",
                    RegistrationNumber = "PER260901006",
                    PhoneNumber = "05320000006",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 107,
                    FirstName = "Burak",
                    LastName = "Aydın",
                    Email = "burak.aydin@fikirhavuzu.com",
                    TCNo = "22222222227",
                    RegistrationNumber = "PER260901007",
                    PhoneNumber = "05320000007",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 108,
                    FirstName = "Seda",
                    LastName = "Arslan",
                    Email = "seda.arslan@fikirhavuzu.com",
                    TCNo = "22222222228",
                    RegistrationNumber = "PER260901008",
                    PhoneNumber = "05320000008",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 109,
                    FirstName = "Emre",
                    LastName = "Yıldız",
                    Email = "emre.yildiz@fikirhavuzu.com",
                    TCNo = "22222222229",
                    RegistrationNumber = "PER260901009",
                    PhoneNumber = "05320000009",
                    PasswordHash = "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO",
                    PasswordSalt = "STATIC_SALT",
                    IsActive = true,
                    IsPasswordChangeRequired = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            modelBuilder.Entity<Idea>().HasData(
                new Idea
                {
                    Id = 101,
                    Title = "Yapay Zeka Destekli Müşteri Destek Asistanı",
                    IntendedBenefit = "Müşteri memnuniyetini artırmak ve destek talebi yanıt sürelerini %40 kısaltmak.",
                    Description = "7/24 hizmet veren, sıkça sorulan soruları otomatik yanıtlayan LLM tabanlı asistan.",
                    CategoryId = 2,
                    UserId = 101,
                    Status = FikirHavuzu.Entity.Enums.IdeaStatus.Implemented,
                    CreatedAt = new DateTime(2026, 1, 15)
                },
                new Idea
                {
                    Id = 102,
                    Title = "Şirket İçi Karbon Ayak İzi ve Sıfır Atık Takip Paneli",
                    IntendedBenefit = "Sürdürülebilirlik hedeflerini görünür kılmak ve kağıt/enerji israfını azaltmak.",
                    Description = "Departman bazlı kaynak tüketimini anlık analiz eden ve tasarruf hedefi koyan dijital gösterge paneli.",
                    CategoryId = 3,
                    UserId = 102,
                    Status = FikirHavuzu.Entity.Enums.IdeaStatus.Implemented,
                    CreatedAt = new DateTime(2026, 1, 20)
                },
                new Idea
                {
                    Id = 103,
                    Title = "Yemekhane Menü Seçimi ve İsraf Önleme Mobil Uygulaması",
                    IntendedBenefit = "Yemekhane gıda israfını engellemek, günlük porsiyon planlamasını veriye dayalı yapmak.",
                    Description = "Çalışanların ertesi gün yemek tercihlerini önceden bildirdiği mobil anket ve takip arayüzü.",
                    CategoryId = 3,
                    UserId = 105,
                    Status = FikirHavuzu.Entity.Enums.IdeaStatus.Implemented,
                    CreatedAt = new DateTime(2026, 2, 1)
                },
                new Idea
                {
                    Id = 104,
                    Title = "Otomatik Kod İnceleme ve Güvenlik Taraması CI/CD Botu",
                    IntendedBenefit = "Yazılım geliştirme süreçlerinde kod kalitesini artırmak ve zafiyetleri erken tespit etmek.",
                    Description = "Git commit ve PR işlemlerinde statik kod analizi yapan otomasyon botu.",
                    CategoryId = 1,
                    UserId = 103,
                    Status = FikirHavuzu.Entity.Enums.IdeaStatus.Approved,
                    CreatedAt = new DateTime(2026, 2, 10)
                },
                new Idea
                {
                    Id = 105,
                    Title = "Zero-Trust Mimarili Şirket İçi Dosya Paylaşım Kasası",
                    IntendedBenefit = "Hassas kurum verilerinin güvenliğini uçtan uca şifreleme ile korumak.",
                    Description = "Çalışanlar arası güvenli, süre kısıtlamalı ve erişim loglu dosya paylaşım ortamı.",
                    CategoryId = 1,
                    UserId = 104,
                    Status = FikirHavuzu.Entity.Enums.IdeaStatus.Approved,
                    CreatedAt = new DateTime(2026, 2, 15)
                }
            );

            modelBuilder.Entity<Evaluation>().HasData(
                new Evaluation
                {
                    Id = 101,
                    IdeaId = 101,
                    EvaluatorUserId = 1,
                    Decision = FikirHavuzu.Entity.Enums.EvaluationDecision.Positive,
                    Score = 95,
                    Comment = "Harika bir yenilikçi proje, dijitalleşme vizyonumuza son derece uygun.",
                    Status = FikirHavuzu.Entity.Enums.EvaluationStatus.Approved,
                    ApprovedAt = new DateTime(2026, 2, 1),
                    CreatedAt = new DateTime(2026, 2, 1)
                },
                new Evaluation
                {
                    Id = 102,
                    IdeaId = 102,
                    EvaluatorUserId = 1,
                    Decision = FikirHavuzu.Entity.Enums.EvaluationDecision.Positive,
                    Score = 90,
                    Comment = "Sürdürülebilirlik ve yeşil ofis hedeflerimiz açısından örnek bir çalışma.",
                    Status = FikirHavuzu.Entity.Enums.EvaluationStatus.Approved,
                    ApprovedAt = new DateTime(2026, 2, 5),
                    CreatedAt = new DateTime(2026, 2, 5)
                },
                new Evaluation
                {
                    Id = 103,
                    IdeaId = 103,
                    EvaluatorUserId = 1,
                    Decision = FikirHavuzu.Entity.Enums.EvaluationDecision.Positive,
                    Score = 88,
                    Comment = "Gıda ve kaynak israfını önleyecek çok pratik bir uygulama.",
                    Status = FikirHavuzu.Entity.Enums.EvaluationStatus.Approved,
                    ApprovedAt = new DateTime(2026, 2, 10),
                    CreatedAt = new DateTime(2026, 2, 10)
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
