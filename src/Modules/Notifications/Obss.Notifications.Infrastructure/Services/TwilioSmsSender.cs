using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.Notifications.Application.Abstractions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Obss.Notifications.Infrastructure.Services;

public sealed class TwilioSmsSender : ISmsSender
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(IOptions<TwilioOptions> options, ILogger<TwilioSmsSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_options.FromNumber),
                to: new PhoneNumber(phoneNumber));

            _logger.LogInformation("SMS sent to {PhoneNumber}: SID={Sid}", phoneNumber, result.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}

public sealed class TwilioOptions
{
    public const string SectionName = "Twilio";
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}
