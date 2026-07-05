using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetCreditProfiles;

public sealed record GetCreditProfilesQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<CreditProfileDto>>>;
