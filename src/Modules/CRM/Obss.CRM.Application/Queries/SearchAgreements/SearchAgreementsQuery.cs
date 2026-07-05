using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.SearchAgreements;

public sealed record SearchAgreementsQuery(
    Guid? CustomerId,
    string? Status) : IRequest<Result<List<AgreementDto>>>;
