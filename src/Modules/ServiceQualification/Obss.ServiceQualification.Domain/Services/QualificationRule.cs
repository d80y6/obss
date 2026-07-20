namespace Obss.ServiceQualification.Domain.Services;

public sealed record QualificationRule(
    string ServiceType,
    string Segment,
    Func<UnifiedQualificationRequest, bool> EligibilityCheck,
    int? MaxDistanceFromExchange,
    int? MinSpeedMbps,
    int? MaxSpeedMbps,
    bool RequiresLineOfSight,
    string TechnologyDescription,
    string TechnologyDescriptionAr);
