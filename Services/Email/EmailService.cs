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
    private readonly static AsyncRetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, _) =>
                {
                    Console.WriteLine(
                        $"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry."
                    );
                }
            );
}
