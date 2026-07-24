namespace Obss.AAA.Domain.ValueObjects;

public enum AaaEventType
{
    AuthenticationSuccess = 1,
    AuthenticationFailure = 2,
    AccountingStart = 3,
    AccountingStop = 4,
    AccountingInterim = 5,
    NasRegistered = 6,
    NasUpdated = 7,
    NasDeleted = 8,
    NasStatusChanged = 9
}
