using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetComplianceSummary;

public sealed record GetComplianceSummaryQuery : IRequest<Result<ComplianceSummaryDto>>;
