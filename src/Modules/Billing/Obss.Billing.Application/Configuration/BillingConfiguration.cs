namespace Obss.Billing.Application.Configuration;

public sealed class BillingConfiguration
{
    public string DefaultCurrency { get; set; } = "YER";
    public IReadOnlyList<string> SupportedCurrencies { get; set; } = ["YER", "USD", "SAR", "EUR"];
    public int DecimalPlaces { get; set; } = 2;
    public string DefaultLocale { get; set; } = "ar-YE";
    public string Timezone { get; set; } = "Asia/Aden";
    public decimal TaxRate { get; set; } = 0.05m;
    public bool EnableRounding { get; set; } = true;
}
