using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillById;

public sealed record GetBillByIdQuery(Guid BillId) : IRequest<Result<BillDto>>;
