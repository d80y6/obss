using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Abstractions;

namespace Obss.Notifications.Infrastructure.Services;

public sealed class SmsService : ISmsSender
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[MOCK] Sending SMS to {PhoneNumber}: '{Message}'",
            phoneNumber, message);

        return Task.FromResult(true);
    }
}
