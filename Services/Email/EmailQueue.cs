using System.Collections.Concurrent;

namespace Chime_ASPNET.Services.Email;

public class EmailQueue(ILogger<EmailQueue> logger)
{
    private readonly ConcurrentQueue<(IEnumerable<string> emails, string subject, string content)> _emailQueue = new();

    public void QueueEmail(IEnumerable<string> emails, string subject, string content)
    {
        var emailList = emails.Where(email => email.Contains('@')).ToList();

        logger.LogInformation("Queueing email to {count} recipients.", emailList.Count);

        if (emailList.Count == 0) return;

        _emailQueue.Enqueue((emailList, subject, content));
    }

    public bool TryDequeue(out (IEnumerable<string> emails, string subject, string content) email)
    {
        return _emailQueue.TryDequeue(out email);
    }
}
