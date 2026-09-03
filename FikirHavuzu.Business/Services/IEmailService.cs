namespace FikirHavuzu.Business.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendWelcomeCredentialsAsync(string toEmail, string fullName, string registrationNumber, string temporaryPassword, string loginLink);
        Task<bool> SendPasswordResetLinkAsync(string toEmail, string fullName, string resetLink);
    }
}
