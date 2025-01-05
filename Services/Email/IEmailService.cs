namespace Chime_ASPNET.Services.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(IEnumerable<string> emails, string subject, string content);
}
