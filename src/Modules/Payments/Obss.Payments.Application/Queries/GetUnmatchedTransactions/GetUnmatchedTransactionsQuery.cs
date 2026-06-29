using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetUnmatchedTransactions;

public sealed record GetUnmatchedTransactionsQuery : IRequest<Result<List<ReconciliationItemDto>>>;
