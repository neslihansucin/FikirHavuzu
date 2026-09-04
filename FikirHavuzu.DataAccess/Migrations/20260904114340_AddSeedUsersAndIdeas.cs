using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FikirHavuzu.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedUsersAndIdeas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "IsPasswordChangeRequired", "LastName", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiration", "PasswordSalt", "PhoneNumber", "ProfilePictureUrl", "RegistrationNumber", "TCNo", "UpdatedAt" },
                values: new object[,]
                {
                    { 101, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "mehmet.yilmaz@fikirhavuzu.com", "Mehmet", true, false, "Yılmaz", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000001", null, "PER260901001", "22222222221", null },
                    { 102, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayse.kaya@fikirhavuzu.com", "Ayşe", true, false, "Kaya", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000002", null, "PER260901002", "22222222222", null },
                    { 103, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ali.demir@fikirhavuzu.com", "Ali", true, false, "Demir", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000003", null, "PER260901003", "22222222223", null },
                    { 104, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "zeynep.celik@fikirhavuzu.com", "Zeynep", true, false, "Çelik", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000004", null, "PER260901004", "22222222224", null },
                    { 105, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "mustafa.sahin@fikirhavuzu.com", "Mustafa", true, false, "Şahin", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000005", null, "PER260901005", "22222222225", null },
                    { 106, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "elif.ozturk@fikirhavuzu.com", "Elif", true, false, "Öztürk", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000006", null, "PER260901006", "22222222226", null },
                    { 107, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "burak.aydin@fikirhavuzu.com", "Burak", true, false, "Aydın", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000007", null, "PER260901007", "22222222227", null },
                    { 108, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seda.arslan@fikirhavuzu.com", "Seda", true, false, "Arslan", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000008", null, "PER260901008", "22222222228", null },
                    { 109, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "emre.yildiz@fikirhavuzu.com", "Emre", true, false, "Yıldız", "$2a$11$jsStJH8UNbA.VY5W2YC2OulejfCal326xhzuQhYYp0EnYsLkLyqVO", null, null, "STATIC_SALT", "05320000009", null, "PER260901009", "22222222229", null }
                });

            migrationBuilder.InsertData(
                table: "Ideas",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IntendedBenefit", "IsEdited", "Status", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 101, 2, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "7/24 hizmet veren, sıkça sorulan soruları otomatik yanıtlayan LLM tabanlı asistan.", "Müşteri memnuniyetini artırmak ve destek talebi yanıt sürelerini %40 kısaltmak.", false, 4, "Yapay Zeka Destekli Müşteri Destek Asistanı", null, 101 },
                    { 102, 3, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Departman bazlı kaynak tüketimini anlık analiz eden ve tasarruf hedefi koyan dijital gösterge paneli.", "Sürdürülebilirlik hedeflerini görünür kılmak ve kağıt/enerji israfını azaltmak.", false, 4, "Şirket İçi Karbon Ayak İzi ve Sıfır Atık Takip Paneli", null, 102 },
                    { 103, 3, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Çalışanların ertesi gün yemek tercihlerini önceden bildirdiği mobil anket ve takip arayüzü.", "Yemekhane gıda israfını engellemek, günlük porsiyon planlamasını veriye dayalı yapmak.", false, 4, "Yemekhane Menü Seçimi ve İsraf Önleme Mobil Uygulaması", null, 105 },
                    { 104, 1, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Git commit ve PR işlemlerinde statik kod analizi yapan otomasyon botu.", "Yazılım geliştirme süreçlerinde kod kalitesini artırmak ve zafiyetleri erken tespit etmek.", false, 2, "Otomatik Kod İnceleme ve Güvenlik Taraması CI/CD Botu", null, 103 },
                    { 105, 1, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Çalışanlar arası güvenli, süre kısıtlamalı ve erişim loglu dosya paylaşım ortamı.", "Hassas kurum verilerinin güvenliğini uçtan uca şifreleme ile korumak.", false, 2, "Zero-Trust Mimarili Şirket İçi Dosya Paylaşım Kasası", null, 104 }
                });

            migrationBuilder.InsertData(
                table: "Evaluations",
                columns: new[] { "Id", "ApprovedAt", "Comment", "CreatedAt", "Decision", "EvaluatorUserId", "IdeaId", "Score", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 101, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Harika bir yenilikçi proje, dijitalleşme vizyonumuza son derece uygun.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 101, 95, 1, null },
                    { 102, new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sürdürülebilirlik ve yeşil ofis hedeflerimiz açısından örnek bir çalışma.", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 102, 90, 1, null },
                    { 103, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gıda ve kaynak israfını önleyecek çok pratik bir uygulama.", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 103, 88, 1, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Ideas",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Ideas",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Ideas",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Ideas",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Ideas",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uFp.3fQZzX7rGqF0Gq9l5e1ZKp3gN8sW8hXJ8yD7gL9kP1mQ2r3tS");
        }
    }
}
