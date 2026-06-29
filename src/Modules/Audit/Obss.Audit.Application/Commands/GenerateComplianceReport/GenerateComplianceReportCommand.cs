using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.GenerateComplianceReport;

public sealed record GenerateComplianceReportCommand : IRequest<Result<ComplianceReportDto>>;
