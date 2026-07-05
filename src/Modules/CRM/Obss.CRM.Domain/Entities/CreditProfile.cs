using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Domain.Entities;

public class CreditProfile : Entity<Guid>
{
    private CreditProfile() { }

    public CreditProfile(
        Guid id,
        Guid customerId,
        int score,
        string scoreType,
        TimePeriod validFor,
        string? riskRating)
        : base(id)
    {
        CustomerId = customerId;
        Score = score;
        ScoreType = scoreType;
        ValidFor = validFor;
        RiskRating = riskRating;
    }

    public Guid CustomerId { get; private set; }
    public int Score { get; private set; }
    public string ScoreType { get; private set; } = string.Empty;
    public TimePeriod ValidFor { get; private set; } = new(null, null);
    public string? RiskRating { get; private set; }
}
