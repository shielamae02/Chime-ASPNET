namespace Chime_ASPNET.Services.Auth;

public class AuthBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<AuthBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Auth background service has started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var time = DateTime.UtcNow;

                if (time.Hour == 0 && time.Minute == 0)
                {
                    logger.LogInformation("Attempting to delete revoked tokens.");

                    using var scope = serviceProvider.CreateScope();
                    var tokenService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    await tokenService.CleanUpTokensAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogError("Auth background service is stopping due to cancellation.");
            }
        }
    }
}
