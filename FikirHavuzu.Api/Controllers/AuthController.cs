using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IIdeaService _ideaService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IEmailService emailService, IIdeaService ideaService, IConfiguration configuration)
        {
            _userService = userService;
            _emailService = emailService;
            _ideaService = ideaService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.RegistrationNumber) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Sicil numarası ve şifre boş bırakılamaz." });

            var user = await _userService.AuthenticateAsync(request.RegistrationNumber, request.Password);

            if (user == null)
                return Unauthorized(new { message = "Sicil numarası veya şifre hatalı." });

            if (!user.IsActive)
                return Unauthorized(new { message = "Hesabınız pasif durumdadır. Lütfen sistem yöneticinizle iletişime geçin." });

            var token = GenerateJwtToken(user.Id, user.Email, $"{user.FirstName} {user.LastName}");

            var userPermIds = await _userService.GetUserPermissionIdsAsync(user.Id);
            var allPerms = await _userService.GetAllPermissionsAsync();
            var userPermissions = allPerms.Where(p => userPermIds.Contains(p.Id)).Select(p => p.Name).ToList();

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    registrationNumber = user.RegistrationNumber,
                    profilePictureUrl = user.ProfilePictureUrl,
                    isPasswordChangeRequired = user.IsPasswordChangeRequired,
                    permissions = userPermissions
                }
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.RegistrationNumber))
                return BadRequest(new { message = "E-posta veya sicil numarası gereklidir." });

            var allUsers = await _userService.GetAllUsersAsync();
            FikirHavuzu.Entity.Entities.User? user = null;

            if (!string.IsNullOrEmpty(request.Email))
                user = allUsers.FirstOrDefault(u => u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));
            else if (!string.IsNullOrEmpty(request.RegistrationNumber))
                user = allUsers.FirstOrDefault(u => u.RegistrationNumber.Equals(request.RegistrationNumber.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user == null || !user.IsActive)
                return NotFound(new { message = "Bu bilgilerle eşleşen aktif bir personel bulunamadı." });

            string token = Guid.NewGuid().ToString("N");
            DateTime expiration = DateTime.UtcNow.AddMinutes(10);

            bool isSaved = await _userService.SavePasswordResetTokenAsync(user.Email, token, expiration);
            if (!isSaved)
                return StatusCode(500, new { message = "Token oluşturulamadı." });

            string resetLink = $"{request.BaseUrl}/auth/reset-password?email={user.Email}&token={token}";
            await _emailService.SendPasswordResetLinkAsync(user.Email, $"{user.FirstName} {user.LastName}", resetLink);

            return Ok(new { message = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token) ||
                string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest(new { message = "Eksik bilgi gönderildi." });

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { message = "Girdiğiniz şifreler eşleşmiyor." });

            if (!IsPasswordComplex(request.NewPassword))
                return BadRequest(new { message = "Şifre en az 8 karakter olmalı; büyük/küçük harf, rakam ve özel karakter içermelidir." });

            var user = await _userService.ValidatePasswordResetTokenAsync(request.Email, request.Token);
            if (user == null)
                return BadRequest(new { message = "Token geçersiz veya süresi dolmuş." });

            bool isSuccess = await _userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!isSuccess)
                return StatusCode(500, new { message = "Şifre sıfırlama başarısız oldu." });

            return Ok(new { message = "Şifreniz başarıyla sıfırlandı." });
        }

        [HttpPost("change-password")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest(new { message = "Şifre alanları boş bırakılamaz." });

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { message = "Şifreler birbiriyle uyuşmuyor." });

            if (!IsPasswordComplex(request.NewPassword))
                return BadRequest(new { message = "Şifre en az 8 karakter olmalı; büyük/küçük harf, rakam ve özel karakter içermelidir." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null && BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                return BadRequest(new { message = "Yeni şifreniz mevcut şifrenizle aynı olamaz." });

            bool isSuccess = await _userService.ChangePasswordAsync(userId, request.NewPassword);
            if (!isSuccess)
                return StatusCode(500, new { message = "Şifre güncellenirken bir hata oluştu." });

            return Ok(new { message = "Şifreniz başarıyla güncellendi." });
        }

        [HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var allIdeas = await _ideaService.GetAllIdeasAsync();

            var myIdeas = allIdeas.Where(i => i.UserId == userId).ToList();
            var myPermIds = await _userService.GetUserPermissionIdsAsync(userId);
            var allPerms = await _userService.GetAllPermissionsAsync();

            int approvedCount = myIdeas.Count(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented);
            int implementedCount = myIdeas.Count(i => i.Status == IdeaStatus.Implemented);
            int myPoints = (approvedCount * 50) + (implementedCount * 100);

            var leaderboard = allIdeas
                .Where(i => i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Implemented)
                .GroupBy(i => i.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Points = (g.Count() * 50) + (g.Count(x => x.Status == IdeaStatus.Implemented) * 100),
                    JuryScoreSum = g.Sum(x => x.Evaluations.OrderByDescending(e => e.CreatedAt).FirstOrDefault()?.Score ?? 0)
                })
                .OrderByDescending(x => x.Points)
                .ThenByDescending(x => x.JuryScoreSum)
                .ToList();

            int userRank = leaderboard.FindIndex(x => x.UserId == userId);
            bool isLeader = userRank == 0 && myPoints > 0;

            string badgeTitle;
            string badgeIcon;
            if (userRank == 0 && myPoints > 0) { badgeTitle = "İnovasyon Lideri"; badgeIcon = "👑"; }
            else if (userRank == 1 && myPoints > 0) { badgeTitle = "Pırıltılı İnovatör"; badgeIcon = "🌟"; }
            else if (userRank == 2 && myPoints > 0) { badgeTitle = "İnovatif Düşünür"; badgeIcon = "💡"; }
            else { badgeTitle = "Fikir Kaşifi"; badgeIcon = "🌱"; }

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                registrationNumber = user.RegistrationNumber,
                tcNo = user.TCNo,
                profilePictureUrl = user.ProfilePictureUrl,
                totalIdeasCount = myIdeas.Count,
                approvedIdeasCount = approvedCount,
                implementedIdeasCount = implementedCount,
                innovationPoints = myPoints,
                badgeTitle,
                badgeIcon,
                isLeader,
                permissions = allPerms.Where(p => myPermIds.Contains(p.Id)).Select(p => p.Name).ToList()
            });
        }

        [HttpPost("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (request.RemoveProfilePhoto)
            {
                user.ProfilePictureUrl = null;
            }
            else if (request.ProfilePhoto != null && request.ProfilePhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ProfilePhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await request.ProfilePhoto.CopyToAsync(stream);
                user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
            }

            await _userService.UpdateUserAsync(user);
            return Ok(new { message = "Profil başarıyla güncellendi.", profilePictureUrl = user.ProfilePictureUrl });
        }

        private string GenerateJwtToken(int userId, string email, string fullName)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpirationHours"] ?? "24")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            return password.Any(char.IsUpper) && password.Any(char.IsLower)
                && password.Any(char.IsDigit) && password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }

    public record LoginRequest(string RegistrationNumber, string Password);
    public record ForgotPasswordRequest(string? Email, string? RegistrationNumber, string BaseUrl);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmPassword);
    public record ChangePasswordRequest(string NewPassword, string ConfirmPassword);
    public class UpdateProfileRequest
    {
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public bool RemoveProfilePhoto { get; set; }
    }
}
