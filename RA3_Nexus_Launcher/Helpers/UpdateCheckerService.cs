// Services/UpdateCheckerService.cs
using RA3_Nexus_Launcher.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RA3_Nexus_Launcher.Helpers
{
    public class UpdateCheckerService(HttpClient httpClient, string owner, string repo, string currentVersion)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _owner = owner;
        private readonly string _repo = repo;
        private readonly string _currentVersion = currentVersion;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Поле для хранения URL последнего релиза
        private string? _latestReleaseUrl;

        /// <summary>
        /// Проверяет наличие новой версии на GitHub.
        /// </summary>
        /// <returns>URL последнего релиза, если обновление доступно, иначе null.</returns>
        public async Task<string?> CheckForUpdatesAsync()
        {
            try
            {
                string apiUrl = $"https://api.github.com/repos/{_owner}/{_repo}/releases/latest";

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("User-Agent", $"{_owner}-{_repo}-Updater");

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    var release = JsonSerializer.Deserialize<GitHubRelease>(jsonContent, _jsonOptions);

                    if (release != null)
                    {
                        // Сохраняем URL релиза
                        _latestReleaseUrl = release.HtmlUrl;

                        // Сравниваем версии
                        if (CompareVersions(release.TagName, _currentVersion) > 0)
                        {
                            // Обновление доступно
                            string message = $"A new version is available: {release.TagName}\nPublished on: {release.PublishedAt:yyyy-MM-dd HH:mm:ss}\n\n{release.Body}";
                            // Показываем уведомление с действием по клику
                            NotificationHelpers.ShowInformation(
                                "Update Available (Click for download)",
                                message,
                                TimeSpan.FromSeconds(10),
                                () => UrlHelper.OpenUrl(release.HtmlUrl) // Действие по клику
                            );
                            return release.HtmlUrl; // Возвращаем URL
                        }
                        else
                        {
                            return null; // Обновления нет
                        }
                    }
                    else
                    {
                        NotificationHelpers.ShowError("Update Check Failed", "Could not parse the latest release information from GitHub.", TimeSpan.FromSeconds(5));
                        return null;
                    }
                }
                else
                {
                    NotificationHelpers.ShowError("Update Check Failed", $"GitHub API returned status code: {response.StatusCode}", TimeSpan.FromSeconds(5));
                    return null;
                }
            }
            catch (HttpRequestException httpEx)
            {
                NotificationHelpers.ShowError("Network Error", $"Could not connect to GitHub: {httpEx.Message}", TimeSpan.FromSeconds(5));
                return null;
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                NotificationHelpers.ShowError("Network Timeout", "The request to GitHub timed out.", TimeSpan.FromSeconds(5));
                return null;
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Update Check Failed", $"An unexpected error occurred: {ex.Message}", TimeSpan.FromSeconds(5));
                return null;
            }
        }

        /// <summary>
        /// Возвращает сохранённый URL последнего релиза.
        /// </summary>
        /// <returns>URL релиза или null, если проверка не проводилась или не нашла релиз.</returns>
        public string? GetLatestReleaseUrl()
        {
            return _latestReleaseUrl;
        }

        private static int CompareVersions(string remoteVersion, string localVersion)
        {
            string cleanRemote = remoteVersion.TrimStart('v', 'V');
            string cleanLocal = localVersion.TrimStart('v', 'V');

            if (Version.TryParse(cleanRemote, out Version? remoteVer) && Version.TryParse(cleanLocal, out Version? localVer))
            {
                return remoteVer.CompareTo(localVer);
            }
            else
            {
                NotificationHelpers.ShowWarning("Version Parse Error", "Could not parse version strings for comparison.", TimeSpan.FromSeconds(4));
                return 0;
            }
        }
    }
}