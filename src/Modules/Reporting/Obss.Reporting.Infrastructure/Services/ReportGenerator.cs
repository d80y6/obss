using Microsoft.Extensions.Logging;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Infrastructure.Services;

public sealed class ReportGenerator : IReportGenerator
{
    private readonly ILogger<ReportGenerator> _logger;

    public ReportGenerator(ILogger<ReportGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<(string FilePath, long FileSize)> GenerateAsync(ReportDefinition reportDefinition, CancellationToken cancellationToken = default)
    {
        var reportsDir = Path.Combine(Path.GetTempPath(), "reports");
        Directory.CreateDirectory(reportsDir);

        var extension = reportDefinition.OutputFormat.ToString().ToLowerInvariant();
        var fileName = $"{reportDefinition.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
        var filePath = Path.Combine(reportsDir, fileName);

        var content = reportDefinition.OutputFormat switch
        {
            Domain.ValueObjects.OutputFormat.PDF => GeneratePdf(reportDefinition),
            Domain.ValueObjects.OutputFormat.Excel => GenerateExcel(reportDefinition),
            Domain.ValueObjects.OutputFormat.CSV => GenerateCsv(reportDefinition),
            Domain.ValueObjects.OutputFormat.HTML => GenerateHtml(reportDefinition),
            _ => GenerateCsv(reportDefinition)
        };

        await File.WriteAllTextAsync(filePath, content, cancellationToken);

        var fileInfo = new FileInfo(filePath);
        var fileSize = fileInfo.Length;

        _logger.LogInformation("Report generated: {FilePath} ({FileSize} bytes)", filePath, fileSize);

        return (filePath, fileSize);
    }

    private static string GeneratePdf(ReportDefinition reportDefinition)
    {
        return $"%PDF-1.4\n1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n4 0 obj\n<< /Length 44 >>\nstream\nBT /F1 12 Tf 100 700 Td ({reportDefinition.Name}) Tj ET\nendstream\nendobj\n5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\nxref\n0 6\n0000000000 65535 f \n0000000009 00000 n \n0000000058 00000 n \n0000000115 00000 n \n0000000266 00000 n \n0000000362 00000 n \ntrailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n437\n%%EOF";
    }

    private static string GenerateExcel(ReportDefinition reportDefinition)
    {
        return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<?mso-application progid=\"Excel.Sheet\"?>\n<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\">\n<Worksheet ss:Name=\"{reportDefinition.Name}\">\n<Table>\n<Row>\n<Cell><Data ss:Type=\"String\">Report: {reportDefinition.Name}</Data></Cell>\n</Row>\n<Row>\n<Cell><Data ss:Type=\"String\">Generated: {DateTime.UtcNow:O}</Data></Cell>\n</Row>\n</Table>\n</Worksheet>\n</Workbook>";
    }

    private static string GenerateCsv(ReportDefinition reportDefinition)
    {
        return $"Report,{reportDefinition.Name}\nGenerated,{DateTime.UtcNow:O}\nType,{reportDefinition.ReportType}\nSource,{reportDefinition.DataSource}\n";
    }

    private static string GenerateHtml(ReportDefinition reportDefinition)
    {
        return $"<!DOCTYPE html>\n<html><head><title>{reportDefinition.Name}</title></head><body>\n<h1>{reportDefinition.Name}</h1>\n<p>Report Type: {reportDefinition.ReportType}</p>\n<p>Data Source: {reportDefinition.DataSource}</p>\n<p>Generated: {DateTime.UtcNow:O}</p>\n</body></html>";
    }
}
