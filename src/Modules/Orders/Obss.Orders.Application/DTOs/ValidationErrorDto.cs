namespace Obss.Orders.Application.DTOs;

public sealed record ValidationErrorDto(string Code, string Message, string Severity);

public sealed record ValidationWarningDto(string Code, string Message);
