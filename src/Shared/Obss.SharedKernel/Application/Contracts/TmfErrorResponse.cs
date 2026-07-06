namespace Obss.SharedKernel.Application.Contracts;

public sealed record TmfErrorResponse
{
    public int Status { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ReferenceError { get; init; }
    public string? SchemaLocation { get; init; }
    public string? Type { get; init; } = "https://tmf-open-api.net/error";

    public static TmfErrorResponse Create(int status, string code, string reason, string message, string? referenceError = null)
    {
        return new TmfErrorResponse
        {
            Status = status,
            Code = code,
            Reason = reason,
            Message = message,
            ReferenceError = referenceError,
            SchemaLocation = $"https://tmf-open-api.net/schemas/error/{status}.json"
        };
    }

    public static TmfErrorResponse FromValidationException(FluentValidation.ValidationException ex)
    {
        var message = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        return Create(400, "VALIDATION_ERROR", "Validation failed", message, null);
    }

    public static TmfErrorResponse FromNotFoundException(string message)
    {
        return Create(404, "NOT_FOUND", "Resource not found", message, null);
    }
}
