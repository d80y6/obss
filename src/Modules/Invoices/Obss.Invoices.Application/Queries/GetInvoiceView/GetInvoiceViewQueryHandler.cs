using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Application.Services;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceView;

public sealed class GetInvoiceViewQueryHandler : IRequestHandler<GetInvoiceViewQuery, Result<InvoiceViewModel>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoicePresenter _invoicePresenter;

    public GetInvoiceViewQueryHandler(
        IInvoiceRepository invoiceRepository,
        IInvoicePresenter invoicePresenter)
    {
        _invoiceRepository = invoiceRepository;
        _invoicePresenter = invoicePresenter;
    }

    public async Task<Result<InvoiceViewModel>> Handle(GetInvoiceViewQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure<InvoiceViewModel>(Error.NotFound(nameof(Invoice), request.InvoiceId));

        var viewModel = await _invoicePresenter.GetViewModelAsync(invoice);
        return Result.Success(viewModel);
    }
}
