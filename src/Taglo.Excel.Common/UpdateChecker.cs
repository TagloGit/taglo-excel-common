using System.Net.Http;
using System.Text.Json;

namespace Taglo.Excel.Common;

/// <summary>
///     Checks for newer releases on GitHub and exposes result for notification.
///     All failures are silent — network errors are logged but never shown to the user.
/// </summary>
public static class UpdateChecker
{
    private static Uri? _latestReleaseUri;
    private static string? _userAgent;

    /// <summary>
    ///     The newer version string (e.g. "0.2.0") if an update is available, otherwise null.
    /// </summary>
    public static string? NewVersionAvailable { get; private set; }

    /// <summary>
    ///     The URL of the GitHub release page for the newer version, otherwise null.
    /// </summary>
    public static string? ReleaseUrl { get; private set; }

    /// <summary>
    ///     Raised when a newer version is detected. Subscribers should refresh their UI
    ///     (e.g. call IRibbonUI.InvalidateControl).
    /// </summary>
    public static event Action? UpdateAvailable;

    /// <summary>
    ///     Configures the update checker with the GitHub repo URL and user-agent string.
    ///     Call once during AddIn.AutoOpen before calling <see cref="CheckForUpdateAsync" />.
    /// </summary>
    /// <param name="repoUrl">
    ///     GitHub API URL for the latest release,
    ///     e.g. "https://api.github.com/repos/TagloGit/taglo-formula-boss/releases/latest"
    /// </param>
    /// <param name="userAgent">
    ///     User-Agent header value, e.g. "FormulaBoss/0.1.0"
    /// </param>
    public static void Initialize(string repoUrl, string userAgent)
    {
        _latestReleaseUri = new Uri(repoUrl);
        _userAgent = userAgent;
        NewVersionAvailable = null;
        ReleaseUrl = null;
    }

    /// <summary>
    ///     Fire-and-forget async check. Call from AddIn.AutoOpen without awaiting.
    /// </summary>
    public static async void CheckForUpdateAsync(Version currentVersion)
    {
        if (_latestReleaseUri == null || _userAgent == null)
        {
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);

            var json = await client.GetStringAsync(_latestReleaseUri);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.GetProperty("tag_name").GetString();
            var htmlUrl = root.GetProperty("html_url").GetString();

            if (tagName == null || htmlUrl == null)
            {
                return;
            }

            var remoteVersion = ParseVersion(tagName);
            if (remoteVersion == null)
            {
                return;
            }

            if (remoteVersion > currentVersion)
            {
                NewVersionAvailable = remoteVersion.ToString();
                ReleaseUrl = htmlUrl;
                Logger.Info($"Update available: v{NewVersionAvailable} (current: v{currentVersion})");
                UpdateAvailable?.Invoke();
            }
            else
            {
                Logger.Info($"No update available (current: v{currentVersion}, latest: v{remoteVersion})");
            }
        }
        catch (Exception ex)
        {
            Logger.Info($"Update check failed (silent): {ex.Message}");
        }
    }

    /// <summary>
    ///     Parses a version string like "v0.2.0" or "0.2.0" into a <see cref="Version" />.
    ///     Returns null if parsing fails.
    /// </summary>
    public static Version? ParseVersion(string tag)
    {
        var cleaned = tag.TrimStart('v', 'V');
        return Version.TryParse(cleaned, out var version) ? version : null;
    }

    /// <summary>
    ///     Resets the update checker state. Intended for testing only.
    /// </summary>
    internal static void Reset()
    {
        _latestReleaseUri = null;
        _userAgent = null;
        NewVersionAvailable = null;
        ReleaseUrl = null;
    }
}
