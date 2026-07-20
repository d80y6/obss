using System.Globalization;
using System.Resources;

namespace Obss.SharedKernel.Application.Services;

public interface ILocalizationService
{
    string GetString(string key);
    string GetString(string key, string culture);
    string FormatMessage(string key, params object[] args);
}

public sealed class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;

    public LocalizationService()
    {
        _resourceManager = new ResourceManager(
            "Obss.SharedKernel.Application.Resources.ServiceMessages",
            typeof(LocalizationService).Assembly);
    }

    public string GetString(string key)
    {
        return _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    public string GetString(string key, string culture)
    {
        try
        {
            var cultureInfo = new CultureInfo(culture);
            return _resourceManager.GetString(key, cultureInfo) ?? key;
        }
        catch (CultureNotFoundException)
        {
            return key;
        }
    }

    public string FormatMessage(string key, params object[] args)
    {
        var msg = GetString(key);
        return string.Format(msg, args);
    }
}
