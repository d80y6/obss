using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Application.DTOs;

public record GeographicAddressDto(
    string Street,
    string City,
    string? State,
    string? PostalCode,
    string Country);

public record AlternateProposalDto(
    Guid ServiceId,
    string ServiceName,
    QualificationResultType ResultType,
    DateTime? EstimatedInstallDate,
    DateTime? GuaranteedUntil);

public record QualificationItemDto(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    QualificationResultType ResultType,
    ServiceQualificationItemState State,
    DateTime? EstimatedInstallDate,
    DateTime? EstimatedCompletionDate,
    string? EligibilityUnavailableReason,
    List<AlternateProposalDto> AlternateProposals);

public record ServiceQualificationDto(
    Guid Id,
    Guid CustomerId,
    GeographicAddressDto Address,
    ServiceQualificationState State,
    DateTime RequestedDate,
    DateTime? ExpirationDate,
    string? Description,
    List<QualificationItemDto> Items);

public record CoverageAreaDto(
    Guid Id,
    string City,
    string? State,
    string? StreetFrom,
    string? StreetTo,
    string? PostalCode,
    List<string> AvailableServices);
