using System.Collections.Concurrent;

namespace Chime_ASPNET.Services.Email;

public class EmailQueue(ILogger<EmailQueue> logger)
{
    private readonly ConcurrentQueue<(IEnumerable<string> emails, string subject, string content)> _emailQueue = new();

}
