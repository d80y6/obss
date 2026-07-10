namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderValidationResultDto(
    bool IsValid,
    List<ValidationErrorDto> Errors,
    List<ValidationWarningDto> Warnings);
