using System.Text;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Application.Services;

public sealed class InvoicePresenter : IInvoicePresenter
{
    public Task<byte[]> GeneratePdfAsync(Invoice invoice)
    {
        var placeholder = Encoding.UTF8.GetBytes($"PDF placeholder for invoice {invoice.InvoiceNumber}");
        return Task.FromResult(placeholder);
    }

    public Task<string> GenerateHtmlAsync(Invoice invoice)
    {
        var linesHtml = new StringBuilder();
        foreach (var line in invoice.Lines)
        {
            linesHtml.AppendFormat(
                "<tr><td>{0}</td><td>{1}</td><td>{2:F2}</td><td>{3:F2}</td></tr>",
                line.Description, line.Quantity, line.UnitPrice, line.TotalAmount);
        }

        var html = $@"
<!DOCTYPE html>
<html>
<head><title>Invoice {invoice.InvoiceNumber}</title></head>
<body>
<h1>Invoice {invoice.InvoiceNumber}</h1>
<p>Customer: {invoice.CustomerName}</p>
<p>Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}</p>
<p>Due Date: {invoice.DueDate:yyyy-MM-dd}</p>
<p>Status: {invoice.Status}</p>
<table border='1' cellpadding='5'>
<thead><tr><th>Description</th><th>Quantity</th><th>Unit Price</th><th>Total</th></tr></thead>
<tbody>{linesHtml}</tbody>
</table>
<h3>Grand Total: {invoice.GrandTotal:F2} {invoice.Currency}</h3>
</body>
</html>";
        return Task.FromResult(html);
    }

    public Task<InvoiceViewModel> GetViewModelAsync(Invoice invoice)
    {
        var viewModel = new InvoiceViewModel(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.CustomerName,
            invoice.CustomerEmail,
            invoice.CustomerAddress,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.Status.ToString(),
            invoice.SubTotal,
            invoice.DiscountTotal,
            invoice.TaxTotal,
            invoice.GrandTotal,
            invoice.AmountPaid,
            invoice.AmountDue,
            invoice.Currency,
            invoice.Notes,
            invoice.CreatedAt,
            invoice.UpdatedAt,
            invoice.SentAt,
            invoice.PaidAt,
            false,
            invoice.Lines.Select(l => new InvoiceViewLineItem(
                l.Id,
                l.Description,
                l.Quantity,
                l.UnitPrice,
                l.TotalAmount,
                l.TaxAmount,
                l.LineType.ToString(),
                l.Currency)).ToList(),
            invoice.Payments.Select(p => new InvoiceViewPayment(
                p.Id,
                p.Amount,
                p.PaymentReference,
                p.PaidAt)).ToList());

        return Task.FromResult(viewModel);
    }
}
