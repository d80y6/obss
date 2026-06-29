using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetUnratedRecords;

public sealed record GetUnratedRecordsQuery : IRequest<Result<IReadOnlyList<UsageRecordDto>>>;
