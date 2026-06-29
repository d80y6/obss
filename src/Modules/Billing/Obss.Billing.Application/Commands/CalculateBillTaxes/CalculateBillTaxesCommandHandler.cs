using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Services;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CalculateBillTaxes;

public sealed class CalculateBillTaxesCommandHandler : IRequestHandler<CalculateBillTaxesCommand, Result<BillDto>>
{
    private readonly IBillRepository _billRepository;
    private readonly ITaxCalculator _taxCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CalculateBillTaxesCommandHandler> _logger;

    public CalculateBillTaxesCommandHandler(
        IBillRepository billRepository,
        ITaxCalculator taxCalculator,
        IUnitOfWork unitOfWork,
        ILogger<CalculateBillTaxesCommandHandler> logger)
    {
        _billRepository = billRepository;
        _taxCalculator = taxCalculator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BillDto>> Handle(CalculateBillTaxesCommand request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdWithLinesAsync(request.BillId, cancellationToken);

        if (bill is null)
            return Result.Failure<BillDto>(Error.NotFound(nameof(Bill), request.BillId));

        var updatedBill = await _taxCalculator.CalculateTaxesAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Taxes calculated for bill {BillId}. Tax total: {TaxTotal} {Currency}",
            updatedBill.Id,
            updatedBill.TaxTotal,
            updatedBill.Currency);

        return Result.Success(updatedBill.Adapt<BillDto>());
    }
}
