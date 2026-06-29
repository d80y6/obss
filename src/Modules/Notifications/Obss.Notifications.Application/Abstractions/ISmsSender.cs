namespace Obss.Notifications.Application.Abstractions;

public interface ISmsSender
{
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
