using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPaymentSummary;

public sealed record GetPaymentSummaryQuery : IRequest<Result<PaymentSummaryDto>>;
