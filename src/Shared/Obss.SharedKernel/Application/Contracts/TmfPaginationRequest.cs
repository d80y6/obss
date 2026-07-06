namespace Obss.SharedKernel.Application.Contracts;

public class TmfPaginationRequest
{
    private int _limit = 20;

    public int Offset { get; set; } = 0;

    public int Limit
    {
        get => _limit;
        set => _limit = value > 100 ? 100 : value;
    }

    public string? Fields { get; set; }
}
