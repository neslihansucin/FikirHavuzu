using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace FikirHavuzu.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpServer"];
                var smtpPortStr = _configuration["EmailSettings:SmtpPort"];
                var smtpUser = _configuration["EmailSettings:SenderEmail"];
                var smtpPass = _configuration["EmailSettings:SenderPassword"];
                var senderName = _configuration["EmailSettings:SenderName"] ?? "Fikir Havuzu";
                var fromAddress = smtpUser ?? "noreply@fikirhavuzu.com";

                if (!string.IsNullOrEmpty(smtpHost) && int.TryParse(smtpPortStr, out int smtpPort) && !string.IsNullOrEmpty(smtpUser))
                {
                    using var message = new MailMessage();
                    message.From = new MailAddress(fromAddress, senderName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = htmlBody;
                    message.IsBodyHtml = true;

                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        Credentials = new NetworkCredential(smtpUser, smtpPass),
                        EnableSsl = true
                    };

                    await client.SendMailAsync(message);
                    _logger.LogInformation("E-posta başarıyla gönderildi: {ToEmail} - Konu: {Subject}", toEmail, subject);
                    return true;
                }
                else
                {
                    _logger.LogInformation("\n================== [ E-POSTA GÖNDERİMİ (SİMÜLASYON) ] ==================\nAlıcı: {ToEmail}\nKonu: {Subject}\nİçerik: {Body}\n=========================================================================", toEmail, subject, htmlBody);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {ToEmail}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendWelcomeCredentialsAsync(string toEmail, string fullName, string registrationNumber, string temporaryPassword, string loginLink)
        {
            string subject = "Fikir Havuzu - Giriş Bilgileriniz";
            string body = $@"
            <div style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 580px; margin: 0 auto; background-color: #FFFFFF; border: 1px solid #E2E8F0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);"">
                <div style=""background: linear-gradient(135deg, #1E3A2B 0%, #3B6B54 100%); padding: 28px; text-align: center; color: #FFFFFF;"">
                    <h2 style=""margin: 0; font-size: 22px; font-weight: 700; letter-spacing: -0.01em;"">🌟 Fikir Havuzu</h2>
                    <p style=""margin: 6px 0 0 0; opacity: 0.9; font-size: 14px;"">Kurumsal İnovasyon Portalı</p>
                </div>
                <div style=""padding: 32px 28px; color: #1E293B; line-height: 1.6;"">
                    <h3 style=""margin-top: 0; color: #1E293B; font-size: 18px;"">Merhaba {fullName},</h3>
                    <p>Fikir Havuzu portalına personel başvurunuz İK tarafından başarıyla onaylanmıştır. Sisteme giriş bilgileriniz aşağıdadır:</p>
                    
                    <div style=""background-color: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; padding: 20px; margin: 24px 0;"">
                        <div style=""margin-bottom: 12px;"">
                            <span style=""color: #64748B; font-size: 13px; text-transform: uppercase; font-weight: 600; display: block;"">Sicil Numaranız</span>
                            <span style=""font-family: monospace; font-size: 18px; font-weight: 700; color: #1E3A2B;"">{registrationNumber}</span>
                        </div>
                        <div>
                            <span style=""color: #64748B; font-size: 13px; text-transform: uppercase; font-weight: 600; display: block;"">Geçici Şifreniz</span>
                            <span style=""font-family: monospace; font-size: 18px; font-weight: 700; color: #3B6B54;"">{temporaryPassword}</span>
                        </div>
                    </div>

                    <p style=""color: #92400E; background-color: #FEF3C7; padding: 12px 16px; border-radius: 6px; font-size: 13px; margin: 20px 0;"">
                        ⚠️ <strong>Güvenlik Notu:</strong> Sisteme ilk giriş yaptığınızda güvenlik sebebiyle bu geçici şifreyi kendi özel kalıcı şifrenizle değiştirmeniz zorunludur.
                    </p>

                    <div style=""text-align: center; margin-top: 30px;"">
                        <a href=""{loginLink}"" style=""display: inline-block; background-color: #3B6B54; color: #FFFFFF; text-decoration: none; padding: 12px 28px; border-radius: 6px; font-weight: 600; font-size: 14px;"">Portala Giriş Yap</a>
                    </div>
                </div>
                <div style=""background-color: #F1F5F9; padding: 16px; text-align: center; font-size: 12px; color: #64748B; border-top: 1px solid #E2E8F0;"">
                    © {DateTime.Now.Year} - <strong>TRtek Yazılım</strong> Fikir Havuzu - Staj Projesi
                </div>
            </div>";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendPasswordResetLinkAsync(string toEmail, string fullName, string resetLink)
        {
            string subject = "Fikir Havuzu - Şifre Sıfırlama Bağlantısı";
            string body = $@"
            <div style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 580px; margin: 0 auto; background-color: #FFFFFF; border: 1px solid #E2E8F0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);"">
                <div style=""background: linear-gradient(135deg, #1E3A2B 0%, #3B6B54 100%); padding: 28px; text-align: center; color: #FFFFFF;"">
                    <h2 style=""margin: 0; font-size: 22px; font-weight: 700; letter-spacing: -0.01em;"">🔑 Fikir Havuzu</h2>
                    <p style=""margin: 6px 0 0 0; opacity: 0.9; font-size: 14px;"">Şifre Sıfırlama Talebi</p>
                </div>
                <div style=""padding: 32px 28px; color: #1E293B; line-height: 1.6;"">
                    <h3 style=""margin-top: 0; color: #1E293B; font-size: 18px;"">Merhaba {fullName},</h3>
                    <p>Fikir Havuzu hesabınız için şifre sıfırlama talebinde bulunuldu. Şifrenizi yenilemek için aşağıdaki butona tıklayabilirsiniz:</p>
                    
                    <div style=""text-align: center; margin: 32px 0;"">
                        <a href=""{resetLink}"" style=""display: inline-block; background-color: #3B6B54; color: #FFFFFF; text-decoration: none; padding: 13px 32px; border-radius: 6px; font-weight: 600; font-size: 15px; box-shadow: 0 2px 6px rgba(59, 107, 84, 0.3);"">Şifremi Sıfırla</a>
                    </div>

                    <p style=""font-size: 13px; color: #64748B;"">
                        Eğer buton çalışmıyorsa aşağıdaki bağlantıyı tarayıcınızın adres çubuğuna yapıştırabilirsiniz:<br/>
                        <a href=""{resetLink}"" style=""color: #3B6B54; word-break: break-all;"">{resetLink}</a>
                    </p>

                    <p style=""color: #92400E; background-color: #FEF3C7; padding: 12px 16px; border-radius: 6px; font-size: 13px; margin: 20px 0;"">
                        ⏳ Bu bağlantı güvenlik sebebiyle <strong>10 dakika</strong> boyunca geçerlidir. Talebi siz yapmadıysanız bu e-postayı dikkate almayınız.
                    </p>
                </div>
                <div style=""background-color: #F1F5F9; padding: 16px; text-align: center; font-size: 12px; color: #64748B; border-top: 1px solid #E2E8F0;"">
                    © {DateTime.Now.Year} - <strong>TRtek Yazılım</strong> Fikir Havuzu - Staj Projesi
                </div>
            </div>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
