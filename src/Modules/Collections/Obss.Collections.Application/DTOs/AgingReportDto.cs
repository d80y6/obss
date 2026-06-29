namespace Obss.Collections.Application.DTOs;

public sealed record AgingBucketDto(
    string BucketName,
    int MinDays,
    int MaxDays,
    int CustomerCount,
    int CaseCount,
    decimal TotalOverdueAmount,
    string Currency);

public sealed record AgingReportDto(
    DateTime GeneratedAt,
    List<AgingBucketDto> Buckets,
    int TotalCustomers,
    int TotalCases,
    decimal GrandTotalOverdue,
    string Currency);
