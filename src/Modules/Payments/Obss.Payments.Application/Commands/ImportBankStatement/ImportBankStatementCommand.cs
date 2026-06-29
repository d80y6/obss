using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.ImportBankStatement;

public sealed record ImportBankStatementCommand(
    string ImportSource,
    string? ImportFileName,
    string Currency,
    List<BankTransactionLine> Transactions) : IRequest<Result<PaymentReconciliationDto>>;
