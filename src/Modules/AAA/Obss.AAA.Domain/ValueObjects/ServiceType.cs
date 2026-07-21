namespace Obss.AAA.Domain.ValueObjects;

public enum ServiceType
{
    Framed = 1,
    CallbackFramed = 2,
    Outbound = 3,
    Administrative = 4,
    NasPrompt = 5,
    AuthenticateOnly = 6,
    CallbackNasPrompt = 7,
    CallCheck = 8,
    CallbackAdministrative = 9
}
