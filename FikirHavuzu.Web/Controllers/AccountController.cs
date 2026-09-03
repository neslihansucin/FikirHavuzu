using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FikirHavuzu.Business.Services;
using FikirHavuzu.Entity.Enums;

namespace FikirHavuzu.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IIdeaService _ideaService;

        public AccountController(IUserService userService, IIdeaService ideaService)
        {
            _userService = userService;
            _ideaService = ideaService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string registrationNumber, string password, bool rememberMe, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrEmpty(registrationNumber) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Sicil numarası ve şifre alanları boş bırakılamaz.";
                return View();
            }

            var user = await _userService.AuthenticateAsync(registrationNumber, password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Sicil numarası veya şifre hatalı.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.ErrorMessage = "Hesabınız pasif durumdadır. Lütfen sistem yöneticinizle iletişime geçin.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (user.IsPasswordChangeRequired)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string? email, string? registrationNumber)
        {
            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(registrationNumber))
            {
                ViewBag.ErrorMessage = "Lütfen e-posta adresinizi veya sicil numaranızı girin.";
                return View();
            }

            var allUsers = await _userService.GetAllUsersAsync();
            FikirHavuzu.Entity.Entities.User? user = null;

            if (!string.IsNullOrEmpty(email))
            {
                user = allUsers.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            else if (!string.IsNullOrEmpty(registrationNumber))
            {
                user = allUsers.FirstOrDefault(u => u.RegistrationNumber.Equals(registrationNumber.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (user == null || !user.IsActive)
            {
                ViewBag.ErrorMessage = "Bu bilgilerle eşleşen aktif bir personel bulunamadı.";
                return View();
            }

            string token = Guid.NewGuid().ToString("N");
            DateTime expiration = DateTime.UtcNow.AddMinutes(10); 
            
            bool isSaved = await _userService.SavePasswordResetTokenAsync(user.Email, token, expiration);

            if (isSaved)
            {
                string resetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token = token }, Request.Scheme) ?? "";
                
                var emailService = HttpContext.RequestServices.GetService(typeof(IEmailService)) as IEmailService;
                if (emailService != null)
                {
                    await emailService.SendPasswordResetLinkAsync(user.Email, $"{user.FirstName} {user.LastName}", resetLink);
                }

                TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
                return RedirectToAction("Login");
            }

            ViewBag.ErrorMessage = "Sıfırlama bağlantısı oluşturulurken bir hata meydana geldi.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _userService.ValidatePasswordResetTokenAsync(email, token);
            if (user == null)
            {
                return View("ResetPasswordExpired");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "Eksik bilgi gönderildi veya şifre alanları boş.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Girdiğiniz şifreler eşleşmiyor.";
                return View();
            }

            if (!IsPasswordComplex(newPassword))
            {
                ViewBag.ErrorMessage = "Yeni şifreniz en az 8 karakter uzunluğunda olmalı; büyük harf, küçük harf, rakam ve özel karakter içermelidir.";
                return View();
            }

            bool isSuccess = await _userService.ResetPasswordAsync(email, token, newPassword);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.ErrorMessage = "Şifre sıfırlama işlemi başarısız oldu veya bağlantının süresi dolmuş.";
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "Lütfen şifre alanlarını doldurun.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Girdiğiniz şifreler birbiriyle uyuşmuyor.";
                return View();
            }

            if (!IsPasswordComplex(newPassword))
            {
                ViewBag.ErrorMessage = "Yeni şifreniz en az 8 karakter uzunluğunda olmalı; en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter (!, @, #, $, vb.) içermelidir.";
                return View();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null && BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Yeni şifreniz mevcut şifrenizle aynı olamaz.";
                return View();
            }

            bool isSuccess = await _userService.ChangePasswordAsync(userId, newPassword);
            if (isSuccess)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Şifre güncellenirken bir hata oluştu.";
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return RedirectToAction("Login", "Account");

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
                    Approved = g.Count(),
                    Implemented = g.Count(x => x.Status == IdeaStatus.Implemented),
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
            if (userRank == 0 && myPoints > 0)
            {
                badgeTitle = "İnovasyon Lideri";
                badgeIcon = "👑";
            }
            else if (userRank == 1 && myPoints > 0)
            {
                badgeTitle = "Pırıltılı İnovatör";
                badgeIcon = "🌟";
            }
            else if (userRank == 2 && myPoints > 0)
            {
                badgeTitle = "İnovatif Düşünür";
                badgeIcon = "💡";
            }
            else
            {
                badgeTitle = "Fikir Kaşifi";
                badgeIcon = "🌱";
            }

            ViewBag.MyPermissions = allPerms.Where(p => myPermIds.Contains(p.Id)).ToList();
            ViewBag.TotalIdeasCount = myIdeas.Count();
            ViewBag.ApprovedIdeasCount = approvedCount;
            ViewBag.ImplementedIdeasCount = implementedCount;
            ViewBag.InnovationPoints = myPoints;
            ViewBag.UserBadgeTitle = badgeTitle;
            ViewBag.UserBadgeIcon = badgeIcon;
            ViewBag.IsLeader = isLeader;

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(string phoneNumber, IFormFile? profilePhoto, bool removeProfilePhoto, string? currentPassword, string? newPassword, string? confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                user.PhoneNumber = phoneNumber;
            }

            if (removeProfilePhoto)
            {
                user.ProfilePictureUrl = null;
            }
            else if (profilePhoto != null && profilePhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePhoto.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
            }

            await _userService.UpdateUserAsync(user);

            if (!string.IsNullOrEmpty(currentPassword) || !string.IsNullOrEmpty(newPassword))
            {
                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    TempData["ErrorMessage"] = "Şifrenizi değiştirmek için mevcut ve yeni şifre alanlarını eksiksiz doldurmalısınız.";
                    return RedirectToAction("Profile");
                }

                if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Yeni şifreler birbiriyle uyuşmuyor.";
                    return RedirectToAction("Profile");
                }

                bool isCurrentValid = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
                if (!isCurrentValid)
                {
                    TempData["ErrorMessage"] = "Mevcut şifrenizi hatalı girdiniz.";
                    return RedirectToAction("Profile");
                }

                if (currentPassword == newPassword)
                {
                    TempData["ErrorMessage"] = "Yeni şifreniz mevcut şifrenizle aynı olamaz.";
                    return RedirectToAction("Profile");
                }

                if (!IsPasswordComplex(newPassword))
                {
                    TempData["ErrorMessage"] = "Yeni şifreniz en az 8 karakter olmalı; en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter (!, @, #, $ vb.) içermelidir.";
                    return RedirectToAction("Profile");
                }

                await _userService.ChangePasswordAsync(userId, newPassword);
            }

            TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }

        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
