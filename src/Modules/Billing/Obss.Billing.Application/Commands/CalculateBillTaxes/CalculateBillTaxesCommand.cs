using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CalculateBillTaxes;

public sealed record CalculateBillTaxesCommand(Guid BillId) : IRequest<Result<BillDto>>;
