namespace Chime_ASPNET.Services.Email;

public class EmailBackgroundService(
    ILogger<EmailBackgroundService> logger,
    IEmailService emailService,
    EmailQueue emailQueue
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stopppingToken)
    {
        logger.LogInformation("Email background service has started.");

        while (!stopppingToken.IsCancellationRequested)
        {
            try
            {
                if (emailQueue.TryDequeue(out var email))
                {
                    var isSuccess = await emailService.SendEmailAsync(
                        email.emails, email.subject, email.content
                    );

                    if (isSuccess)
                    {
                        logger.LogInformation("Email sent successfully.");
                    }
                    else
                    {
                        logger.LogError("Failed to send email. Re-queuing the email.");
                        emailQueue.QueueEmail(email.emails, email.subject, email.content);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stopppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogError("Email background service is stopping due to cancellation.");
            }
        }
    }
}
