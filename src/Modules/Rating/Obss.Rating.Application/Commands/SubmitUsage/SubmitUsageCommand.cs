using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.SubmitUsage;

public sealed record SubmitUsageCommand(
    Guid SubscriptionId,
    Guid ServiceId,
    string RecordType,
    string UsageType,
    DateTime StartTime,
    DateTime EndTime,
    long Duration,
    long Volume,
    string SourceIdentifier,
    string DestinationIdentifier,
    string Currency,
    bool RateImmediately = false) : IRequest<Result<UsageRecordDto>>;
