namespace Obss.Notifications.Application.Abstractions;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
