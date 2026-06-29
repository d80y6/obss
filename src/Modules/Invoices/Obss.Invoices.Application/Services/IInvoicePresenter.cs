using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Application.DTOs;

namespace Obss.Invoices.Application.Services;

public interface IInvoicePresenter
{
    Task<byte[]> GeneratePdfAsync(Invoice invoice);
    Task<string> GenerateHtmlAsync(Invoice invoice);
    Task<InvoiceViewModel> GetViewModelAsync(Invoice invoice);
}
