using System.Net.Http.Headers;
using System.Text;
using MediaPager.App.PluginContracts;

namespace MediaPager.Plugins.Email.MailGun;

/// <summary>
/// Official Mailgun email provider: delivers through the Mailgun HTTP messages API.
/// Settings live under plugins.mailgun.* and flow through the SDK's IPluginSettingsStore;
/// config is the fallback. Settings are read per call so a runtime update takes effect on
/// the next send.
/// </summary>
public sealed class MailgunEmailPlugin(IPluginSettingsStore settingsStore) : IMediaPagerPlugin, IPluginSettingsSchema, IEmailProviderPlugin
{
    public const string PluginKey = "mailgun";
    public const string ApiKeySetting = "apiKey";
    public const string DomainSetting = "domain";
    public const string FromSetting = "from";
    public const string ApiBaseUrlSetting = "apiBaseUrl";

    public const string DefaultApiBaseUrl = "https://api.mailgun.net";

    public PluginDescriptor Descriptor { get; } = new(
        Id: "mediapager.email.mailgun",
        Name: "Mailgun",
        Version: "0.1.0",
        Author: "MediaPager",
        Description: "Send email through the Mailgun messages API.");

    public IReadOnlyList<PluginSettingDefinition> Settings { get; } =
    [
        new PluginSettingDefinition(ApiKeySetting, "Mailgun API key", PluginSettingType.Password, Required: true, Secret: true),
        new PluginSettingDefinition(DomainSetting, "Mailgun sending domain", PluginSettingType.String, Required: true),
        new PluginSettingDefinition(FromSetting, "From address", PluginSettingType.String, Required: true),
        new PluginSettingDefinition(ApiBaseUrlSetting, "Mailgun API base URL", PluginSettingType.String, Default: DefaultApiBaseUrl),
    ];

    public async Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default) =>
        !string.IsNullOrWhiteSpace(await settingsStore.GetAsync(PluginKey, ApiKeySetting, cancellationToken)) &&
        !string.IsNullOrWhiteSpace(await settingsStore.GetAsync(PluginKey, DomainSetting, cancellationToken)) &&
        !string.IsNullOrWhiteSpace(await settingsStore.GetAsync(PluginKey, FromSetting, cancellationToken));

    public async Task<bool> SendAsync(string recipient, string subject, string text, string html,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await settingsStore.GetAsync(PluginKey, ApiKeySetting, cancellationToken);
        var domain = await settingsStore.GetAsync(PluginKey, DomainSetting, cancellationToken);
        var from = await settingsStore.GetAsync(PluginKey, FromSetting, cancellationToken);
        var apiBaseUrl = await settingsStore.GetAsync(PluginKey, ApiBaseUrlSetting, cancellationToken) ?? DefaultApiBaseUrl;
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(from))
            return false;

        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiBaseUri) || apiBaseUri.Scheme != Uri.UriSchemeHttps)
        {
            Console.Error.WriteLine("[email] Mailgun apiBaseUrl must be an HTTPS URL.");
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post,
            new Uri(apiBaseUri, $"/v3/{Uri.EscapeDataString(domain)}/messages"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{apiKey}")));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["from"] = from,
            ["to"] = recipient,
            ["subject"] = subject,
            ["text"] = text,
            ["html"] = html,
        });

        try
        {
            using var response = await Client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return true;
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.Error.WriteLine($"[email] Mailgun returned {(int)response.StatusCode}: " +
                responseText[..Math.Min(responseText.Length, 400)]);
            return false;
        }
        catch (HttpRequestException exception)
        {
            Console.Error.WriteLine($"[email] Mailgun request failed: {exception.Message}");
            return false;
        }
    }

    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(30) };
}
