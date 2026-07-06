namespace Obss.SharedKernel.Application.Contracts;

public class TmfPaginationResponse
{
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public int ResultCount { get; set; }
    public string? NextUrl { get; set; }
    public string? PreviousUrl { get; set; }

    public void SetPaginationHeaders(int offset, int limit, int totalCount)
    {
        Offset = offset;
        Limit = limit;
        TotalCount = totalCount;
        ResultCount = Math.Min(limit, totalCount - offset > 0 ? totalCount - offset : 0);

        NextUrl = (offset + limit) < totalCount
            ? $"?offset={offset + limit}&limit={limit}"
            : null;

        PreviousUrl = offset > 0
            ? $"?offset={Math.Max(0, offset - limit)}&limit={limit}"
            : null;
    }
}
