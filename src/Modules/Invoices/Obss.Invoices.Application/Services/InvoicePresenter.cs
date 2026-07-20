using System.Text;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Obss.Invoices.Application.Services;

public sealed class InvoicePresenter : IInvoicePresenter
{
    public Task<byte[]> GeneratePdfAsync(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ").FontSize(8);
                    x.CurrentPageNumber().FontSize(8);
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdf);
    }

    private static void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Text($"INVOICE {invoice.InvoiceNumber}")
                .FontSize(20).Bold().FontColor(Colors.Blue.Darken3);

            column.Item().Text($"Status: {invoice.Status}");
            column.Item().Text($"Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}");
            column.Item().Text($"Due Date: {invoice.DueDate:yyyy-MM-dd}");
            column.Item().Text($"Customer: {invoice.CustomerName}");
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeContent(IContainer container, Invoice invoice)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Text("Description").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Text("Qty").Bold().AlignRight();
                    header.Cell().Background(Colors.Grey.Lighten3).Text("Unit Price").Bold().AlignRight();
                    header.Cell().Background(Colors.Grey.Lighten3).Text("Total").Bold().AlignRight();
                });

                foreach (var line in invoice.Lines)
                {
                    table.Cell().Text(line.Description);
                    table.Cell().Text(line.Quantity.ToString()).AlignRight();
                    table.Cell().Text($"{line.UnitPrice:F2}").AlignRight();
                    table.Cell().Text($"{line.TotalAmount:F2}").AlignRight();
                }
            });

            column.Item().PaddingTop(20).AlignRight().Column(total =>
            {
                total.Item().Text($"Subtotal: {invoice.SubTotal:F2} {invoice.Currency}");
                if (invoice.DiscountTotal > 0)
                    total.Item().Text($"Discount: -{invoice.DiscountTotal:F2} {invoice.Currency}");
                total.Item().Text($"Tax: {invoice.TaxTotal:F2} {invoice.Currency}");
                total.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                total.Item().Text($"Grand Total: {invoice.GrandTotal:F2} {invoice.Currency}")
                    .FontSize(14).Bold();
                total.Item().Text($"Amount Paid: {invoice.AmountPaid:F2} {invoice.Currency}");
                total.Item().Text($"Amount Due: {invoice.AmountDue:F2} {invoice.Currency}")
                    .FontColor(Colors.Red.Darken2).Bold();
            });

            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().PaddingTop(20).Text($"Notes: {invoice.Notes}");
            }
        });
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
