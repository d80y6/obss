using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;

namespace Obss.Invoices.Application.BackgroundJobs;

public sealed class InvoiceOverdueCheckJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InvoiceOverdueCheckJob> _logger;

    public InvoiceOverdueCheckJob(IServiceScopeFactory scopeFactory, ILogger<InvoiceOverdueCheckJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Invoice overdue check job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOverdueInvoices(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing overdue invoices.");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task ProcessOverdueInvoices(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();

        var overdueInvoices = await invoiceRepository.GetOverdueInvoicesAsync(cancellationToken);

        foreach (var invoice in overdueInvoices)
        {
            try
            {
                invoice.MarkAsOverdue();
                await invoiceRepository.UpdateAsync(invoice, cancellationToken);
                _logger.LogWarning(
                    "Invoice {InvoiceNumber} ({InvoiceId}) marked as overdue.",
                    invoice.InvoiceNumber, invoice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to mark invoice {InvoiceNumber} ({InvoiceId}) as overdue.",
                    invoice.InvoiceNumber, invoice.Id);
            }
        }

        if (overdueInvoices.Count > 0)
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<Obss.SharedKernel.Application.Abstractions.IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
