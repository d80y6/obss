using System.Text.RegularExpressions;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed partial class TelephonyQualificationService : ITelephonyQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public TelephonyQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    [GeneratedRegex(@"^(?:\+?966\d{9}|05\d{8})$")]
    private static partial Regex SaudiPhoneNumberRegex();

    public async Task<TelephonyQualificationResult> QualifyAsync(TelephonyQualificationRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        if (!SaudiPhoneNumberRegex().IsMatch(request.TelephoneNumber.Replace(" ", "").Replace("-", "")))
        {
            return new TelephonyQualificationResult(
                false,
                correlationId,
                $"The telephone number '{request.TelephoneNumber}' is not valid. Saudi numbers must start with +966 or 05 and be 10-12 digits.",
                $"رقم الهاتف '{request.TelephoneNumber}' غير صالح. يجب أن تبدأ الأرقام السعودية بـ +966 أو 05 وتكون من 10-12 رقمًا.",
                ["VoIP service", "Mobile cellular service"],
                ["خدمة VoIP", "خدمة الهاتف المحمول"],
                null,
                null,
                null);
        }

        var address = GeographicAddress.Create(
            request.Address,
            request.City ?? string.Empty,
            request.State,
            null,
            "SA");

        var coverageAreas = await _coverageRepository.GetByAddressAsync(address, ct);

        if (coverageAreas.Count == 0)
        {
            return new TelephonyQualificationResult(
                false,
                correlationId,
                "No telephony coverage found at this address. Consider VoIP or mobile alternatives.",
                "لا توجد تغطية هاتفية في هذا العنوان. يُرجى النظر في بدائل VoIP أو الهاتف المحمول.",
                ["VoIP service", "Mobile cellular service"],
                ["خدمة VoIP", "خدمة الهاتف المحمول"],
                null,
                null,
                null);
        }

        var telephonyService = coverageAreas
            .SelectMany(ca => ca.AvailableServices)
            .FirstOrDefault(s =>
                s.Technology.Equals("POTS", StringComparison.OrdinalIgnoreCase) &&
                s.IsActive);

        if (telephonyService is null)
        {
            var availableTechs = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(s => s.IsActive)
                .Select(s => $"{s.ServiceName} ({s.Technology})")
                .Distinct()
                .ToList();

            return new TelephonyQualificationResult(
                false,
                correlationId,
                $"Traditional telephony (POTS) is not available at this address. Available alternatives: {string.Join(", ", availableTechs)}.",
                $"الهاتف التقليدي (POTS) غير متوفر في هذا العنوان. البدائل المتاحة: {string.Join("، ", availableTechs)}.",
                availableTechs,
                availableTechs.Select(t => t).ToList(),
                null,
                null,
                null);
        }

        var softswitch = await FindSoftswitchAsync(ct);
        var softswitchCapacityOk = softswitch is not null;

        var coverageDetail = new TelephonyCoverageDetail(
            true,
            softswitchCapacityOk,
            request.Segment == "business" ? 2 : 5,
            softswitchCapacityOk
                ? null
                : "Softswitch is at capacity. Number cannot be provisioned at this time.",
            softswitchCapacityOk
                ? null
                : "سعة المقسم البرمجي ممتلئة. لا يمكن توفير الرقم في الوقت الحالي.");

        if (!softswitchCapacityOk)
        {
            return new TelephonyQualificationResult(
                false,
                correlationId,
                "Telephony coverage exists but softswitch capacity is insufficient.",
                "تغطية الهاتف موجودة ولكن سعة المقسم البرمجي غير كافية.",
                null,
                null,
                coverageDetail,
                null,
                ["Softswitch capacity exhausted"]);
        }

        return new TelephonyQualificationResult(
            true,
            correlationId,
            $"Address is qualified for telephony service. Number {request.TelephoneNumber} is available for activation.",
            $"العنوان مؤهل لخدمة الهاتف. الرقم {request.TelephoneNumber} متاح للتفعيل.",
            null,
            null,
            coverageDetail,
            ["Copper pair test and activation", "Telephone line provisioning in softswitch", "CPE phone configuration"],
            null);
    }

    private async Task<NetworkElement?> FindSoftswitchAsync(CancellationToken ct)
    {
        var elements = await _networkElementRepository.GetFilteredAsync(
            "Softswitch", "Active", null, 0, 100, ct);

        return elements.OfType<NetworkElement>().FirstOrDefault();
    }
}
