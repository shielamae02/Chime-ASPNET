using Polly;
using MimeKit;
using Polly.Retry;
using MailKit.Net.Smtp;
using MailKit.Security;
using Chime_ASPNET.Models.Config;

namespace Chime_ASPNET.Services.Email;

public class EmailService(
    ILogger<EmailService> logger,
    SMTPSettings smtp
) : IEmailService
{

}
