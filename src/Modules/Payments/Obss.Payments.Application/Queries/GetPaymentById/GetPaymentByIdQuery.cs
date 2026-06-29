using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IRequest<Result<PaymentDto>>;
