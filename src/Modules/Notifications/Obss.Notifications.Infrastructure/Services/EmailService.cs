using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Abstractions;

namespace Obss.Notifications.Infrastructure.Services;

public sealed class EmailService : IEmailSender
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[MOCK] Sending email to {Recipient}: Subject='{Subject}', Body='{Body}'",
            to, subject, body);

        return Task.FromResult(true);
    }
}
