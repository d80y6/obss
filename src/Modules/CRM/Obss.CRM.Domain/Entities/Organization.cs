using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class Organization : AggregateRoot<Guid>
{
    private Organization() { }

    private Organization(
        Guid id,
        string tradingName,
        CompanyType companyType,
        string? industry,
        string? registrationNumber,
        string? taxNumber,
        string? countryOfRegistration)
        : base(id)
    {
        TradingName = tradingName;
        CompanyType = companyType;
        Industry = industry;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        CountryOfRegistration = countryOfRegistration;
        KycStatus = KycStatus.NotStarted;
    }

    public string TradingName { get; private set; } = string.Empty;
    public CompanyType CompanyType { get; private set; }
    public string? Industry { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? CountryOfRegistration { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTime? KycVerifiedAt { get; private set; }
    public string? KycVerifiedBy { get; private set; }

    public static Organization Create(
        string tradingName,
        CompanyType companyType,
        string? industry = null,
        string? registrationNumber = null,
        string? taxNumber = null,
        string? countryOfRegistration = null)
    {
        return new Organization(
            Guid.NewGuid(), tradingName, companyType,
            industry, registrationNumber, taxNumber, countryOfRegistration);
    }

    public void UpdateDetails(
        string tradingName, CompanyType companyType, string? industry,
        string? registrationNumber, string? taxNumber, string? countryOfRegistration)
    {
        TradingName = tradingName;
        CompanyType = companyType;
        Industry = industry;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        CountryOfRegistration = countryOfRegistration;
    }

    public void VerifyKyc(string verifiedBy)
    {
        KycStatus = KycStatus.Verified;
        KycVerifiedAt = DateTime.UtcNow;
        KycVerifiedBy = verifiedBy;
    }

    public void RejectKyc()
    {
        KycStatus = KycStatus.Rejected;
    }
}
