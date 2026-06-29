using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Services;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.GenerateBill;

public sealed class GenerateBillCommandHandler : IRequestHandler<GenerateBillCommand, Result<BillDto>>
{
    private readonly IBillingCalculator _billingCalculator;
    private readonly ITaxCalculator _taxCalculator;
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateBillCommandHandler> _logger;

    public GenerateBillCommandHandler(
        IBillingCalculator billingCalculator,
        ITaxCalculator taxCalculator,
        IBillRepository billRepository,
        IUnitOfWork unitOfWork,
        ILogger<GenerateBillCommandHandler> logger)
    {
        _billingCalculator = billingCalculator;
        _taxCalculator = taxCalculator;
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BillDto>> Handle(GenerateBillCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BillingPeriod>(request.BillingPeriod, true, out var _))
        {
            return Result.Failure<BillDto>(Error.Validation($"Invalid billing period: '{request.BillingPeriod}'."));
        }

        var bill = await _billingCalculator.CalculateBill(
            request.CustomerId,
            request.PeriodStart,
            request.PeriodEnd,
            cancellationToken);

        bill = await _taxCalculator.CalculateTaxesAsync(bill, cancellationToken);

        await _billRepository.AddAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Bill {BillId} generated for customer {CustomerId} with grand total {GrandTotal} {Currency}",
            bill.Id,
            bill.CustomerId,
            bill.GrandTotal,
            bill.Currency);

        return Result.Success(bill.Adapt<BillDto>());
    }
}
