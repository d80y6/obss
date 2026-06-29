namespace Obss.Orders.Application.DTOs;

public sealed record OrderValidationResultDto(
    bool IsValid,
    List<ValidationErrorDto> Errors,
    List<ValidationWarningDto> Warnings);
