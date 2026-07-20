using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class TelephonyOrderDecompositionService : ITelephonyOrderDecompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<TelephonyDecompositionResult> DecomposeAsync(TelephonyDecompositionRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var serviceTasks = new List<ServiceTask>();
        var resourceTasks = new List<ResourceTask>();

        int stepOrder = 1;

        resourceTasks.Add(new ResourceTask(
            "TEL_NUMBER_RESERVATION",
            "PHONE_NUMBER",
            stepOrder++,
            null,
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                phoneNumber = request.PhoneNumber,
                numberType = "GEOGRAPHIC",
                reservationPeriodHours = 72
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "TEL_NUMBER_ACTIVATION",
            "Number activation",
            "تفعيل الرقم",
            stepOrder++,
            "TEL_NUMBER_RESERVATION",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                phoneNumber = request.PhoneNumber,
                activationType = "IMMEDIATE",
                trunkGroup = "PSTN_TRUNK_1"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "TEL_SUBSCRIBER_PROFILE",
            "Subscriber profile creation",
            "إنشاء ملف المشترك",
            stepOrder++,
            "TEL_NUMBER_ACTIVATION",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                lineType = "POTS",
                billingType = "FLAT_RATE",
                internationalAccess = request.Segment == "business"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "TEL_FEATURE_CONFIG",
            "Feature configuration (call forwarding, waiting, etc.)",
            "تكوين الميزات (تحويل المكالمات، انتظار، إلخ)",
            stepOrder++,
            "TEL_SUBSCRIBER_PROFILE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                callForwarding = request.CallForwarding,
                callForwardingType = "UNCONDITIONAL",
                callWaiting = request.CallWaiting,
                callerId = request.CallerId,
                threeWayCalling = request.ThreeWayCalling,
                voicemailEnabled = true
            }, JsonOptions))));

        if (request.Segment == "business")
        {
            serviceTasks.Add(new ServiceTask(
                "TEL_HUNTING_CONFIG",
                "Hunting group configuration",
                "تكوين مجموعة الصيد",
                stepOrder++,
                "TEL_FEATURE_CONFIG",
                JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    huntingType = "SEQUENTIAL",
                    maxHuntMembers = 5
                }, JsonOptions))));
        }

        serviceTasks.Add(new ServiceTask(
            "TEL_ACTIVATION_TEST",
            "Activation test",
            "اختبار التفعيل",
            stepOrder,
            request.Segment == "business" ? "TEL_HUNTING_CONFIG" : "TEL_FEATURE_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                testTypes = new[] { "DIAL_TONE", "INBOUND_CALL", "OUTBOUND_CALL", "CALLER_ID" }
            }, JsonOptions))));

        return Task.FromResult(new TelephonyDecompositionResult(
            correlationId,
            serviceTasks,
            resourceTasks));
    }
}
