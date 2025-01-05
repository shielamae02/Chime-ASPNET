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

    public async Task<bool> SendEmailAsync(IEnumerable<string> emails, string subject, string content)
    {
        logger.LogInformation("Starting email notification process.");
        var emailList = emails.ToList();

        if (emailList.Count == 0)
        {
            logger.LogWarning("No recipient email addresses provided. Email notification process aborted.");
            return false;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Chime", smtp.Username));

        foreach (var email in emailList)
        {
            message.To.Add(new MailboxAddress("", email));
            logger.LogInformation("Queued email to recipient: {email}", email);
        }

        message.Subject = subject;

        var htmlBody = EmailTemplate.ForgotPasswordTemplate(subject, content);
        message.Body = new TextPart("html") { Text = htmlBody };


        var result = await RetryPolicy.ExecuteAsync(async () =>
        {
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(smtp.Server, smtp.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtp.Username, smtp.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                logger.LogInformation("Email sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "Failed to send email.");
                return false;
            }
        });

        return result;
    }
}
