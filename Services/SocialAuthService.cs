using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Services
{
    public class SocialAuthService
    {
        private static readonly HttpClient Http = new();

        public async Task<(bool success, string message, SocialLoginProfile? profile)> LoginWithGoogleAsync(
            OAuthSettings settings,
            CancellationToken cancellationToken = default)
        {
            if (!settings.IsGoogleConfigured)
            {
                return (false,
                    "Chưa cấu hình Google OAuth. Thêm ClientId trong appsettings.json hoặc appsettings.Development.json.",
                    null);
            }

            try
            {
                var state = Guid.NewGuid().ToString("N");
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);

                var authUrl =
                    "https://accounts.google.com/o/oauth2/v2/auth?" +
                    $"client_id={Uri.EscapeDataString(settings.GoogleClientId)}&" +
                    $"redirect_uri={Uri.EscapeDataString(settings.RedirectUri)}&" +
                    "response_type=code&" +
                    $"scope={Uri.EscapeDataString("openid email profile")}&" +
                    $"state={Uri.EscapeDataString(state)}&" +
                    $"code_challenge={Uri.EscapeDataString(codeChallenge)}&" +
                    "code_challenge_method=S256&" +
                    "access_type=online&" +
                    "prompt=select_account";

                var code = await WaitForAuthorizationCodeAsync(settings, authUrl, state, cancellationToken);
                var accessToken = await ExchangeGoogleCodeAsync(settings, code, codeVerifier, cancellationToken);
                var profile = await FetchGoogleProfileAsync(accessToken, cancellationToken);

                return (true, "Đăng nhập Google thành công.", profile);
            }
            catch (OperationCanceledException)
            {
                return (false, "Đã hủy đăng nhập Google.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Đăng nhập Google thất bại: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, SocialLoginProfile? profile)> LoginWithFacebookAsync(
            OAuthSettings settings,
            CancellationToken cancellationToken = default)
        {
            if (!settings.IsFacebookConfigured)
            {
                return (false,
                    "Chưa cấu hình Facebook OAuth. Thêm AppId và AppSecret trong appsettings.json hoặc appsettings.Development.json.",
                    null);
            }

            try
            {
                var state = Guid.NewGuid().ToString("N");

                var authUrl =
                    "https://www.facebook.com/v21.0/dialog/oauth?" +
                    $"client_id={Uri.EscapeDataString(settings.FacebookAppId)}&" +
                    $"redirect_uri={Uri.EscapeDataString(settings.RedirectUri)}&" +
                    "response_type=code&" +
                    $"scope={Uri.EscapeDataString("email,public_profile")}&" +
                    $"state={Uri.EscapeDataString(state)}";

                var code = await WaitForAuthorizationCodeAsync(settings, authUrl, state, cancellationToken);
                var accessToken = await ExchangeFacebookCodeAsync(settings, code, cancellationToken);
                var profile = await FetchFacebookProfileAsync(accessToken, cancellationToken);

                return (true, "Đăng nhập Facebook thành công.", profile);
            }
            catch (OperationCanceledException)
            {
                return (false, "Đã hủy đăng nhập Facebook.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Đăng nhập Facebook thất bại: {ex.Message}", null);
            }
        }

        private static Task<string> WaitForAuthorizationCodeAsync(
            OAuthSettings settings,
            string authUrl,
            string expectedState,
            CancellationToken cancellationToken)
        {
            return OAuthCallbackServer.WaitForAuthorizationCodeAsync(settings, authUrl, expectedState, cancellationToken);
        }

        private static async Task<string> ExchangeGoogleCodeAsync(
            OAuthSettings settings,
            string code,
            string codeVerifier,
            CancellationToken cancellationToken)
        {
            var body = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = settings.GoogleClientId,
                ["redirect_uri"] = settings.RedirectUri,
                ["grant_type"] = "authorization_code",
                ["code_verifier"] = codeVerifier
            };

            if (!string.IsNullOrWhiteSpace(settings.GoogleClientSecret) &&
                !settings.GoogleClientSecret.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
            {
                body["client_secret"] = settings.GoogleClientSecret;
            }

            using var content = new FormUrlEncodedContent(body);
            using var response = await Http.PostAsync("https://oauth2.googleapis.com/token", content, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(ParseOAuthError(json));

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Google không trả về access token.");
        }

        private static async Task<SocialLoginProfile> FetchGoogleProfileAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await Http.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Không lấy được thông tin tài khoản Google.");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Tài khoản Google chưa cung cấp email.");

            return new SocialLoginProfile
            {
                Provider = AuthProviders.Google,
                ExternalId = root.GetProperty("sub").GetString() ?? string.Empty,
                Email = email.Trim().ToLowerInvariant(),
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null
            };
        }

        private static async Task<string> ExchangeFacebookCodeAsync(
            OAuthSettings settings,
            string code,
            CancellationToken cancellationToken)
        {
            var url =
                "https://graph.facebook.com/v21.0/oauth/access_token?" +
                $"client_id={Uri.EscapeDataString(settings.FacebookAppId)}&" +
                $"redirect_uri={Uri.EscapeDataString(settings.RedirectUri)}&" +
                $"client_secret={Uri.EscapeDataString(settings.FacebookAppSecret)}&" +
                $"code={Uri.EscapeDataString(code)}";

            using var response = await Http.GetAsync(url, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(ParseOAuthError(json));

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Facebook không trả về access token.");
        }

        private static async Task<SocialLoginProfile> FetchFacebookProfileAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            var url =
                "https://graph.facebook.com/v21.0/me?" +
                $"fields=id,name,email&access_token={Uri.EscapeDataString(accessToken)}";

            using var response = await Http.GetAsync(url, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Không lấy được thông tin tài khoản Facebook.");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "Tài khoản Facebook chưa cung cấp email. Hãy cấp quyền email cho ứng dụng hoặc xác minh email trên Facebook.");
            }

            return new SocialLoginProfile
            {
                Provider = AuthProviders.Facebook,
                ExternalId = root.GetProperty("id").GetString() ?? string.Empty,
                Email = email.Trim().ToLowerInvariant(),
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null
            };
        }

        private static string ParseOAuthError(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error_description", out var desc))
                    return desc.GetString() ?? "OAuth error";
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    if (err.ValueKind == JsonValueKind.Object &&
                        err.TryGetProperty("message", out var msg))
                        return msg.GetString() ?? "OAuth error";
                    return err.GetString() ?? "OAuth error";
                }
            }
            catch
            {
                // ignore parse errors
            }

            return "Không thể xác thực với nhà cung cấp đăng nhập.";
        }

        private static string GenerateCodeVerifier()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Base64UrlEncode(bytes);
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(byte[] data)
            => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static Dictionary<string, string> ParseQueryString(string? query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(query))
                return result;

            var trimmed = query.StartsWith('?') ? query[1..] : query;
            foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var separator = part.IndexOf('=');
                if (separator < 0)
                {
                    result[Uri.UnescapeDataString(part)] = string.Empty;
                    continue;
                }

                var key = Uri.UnescapeDataString(part[..separator]);
                var value = Uri.UnescapeDataString(part[(separator + 1)..]);
                result[key] = value;
            }

            return result;
        }
    }
}
