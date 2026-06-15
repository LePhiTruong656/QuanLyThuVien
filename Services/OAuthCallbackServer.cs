using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LibraryManagementFE.Services
{
    internal static class OAuthCallbackServer
    {
        public static async Task<string> WaitForAuthorizationCodeAsync(
            OAuthSettings settings,
            string authUrl,
            string expectedState,
            CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            var builder = WebApplication.CreateSlimBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, settings.RedirectPort, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            var app = builder.Build();
            var callbackPath = NormalizePath(settings.RedirectPath);

            app.MapGet(callbackPath, async context =>
            {
                var query = context.Request.Query;
                var error = query["error"].ToString();
                var errorDescription = query["error_description"].ToString();
                var state = query["state"].ToString();
                var code = query["code"].ToString();

                if (!string.IsNullOrEmpty(error))
                {
                    await WriteHtmlAsync(context, BuildResultPage(false, errorDescription.Length > 0 ? errorDescription : error));
                    tcs.TrySetException(new InvalidOperationException(errorDescription.Length > 0 ? errorDescription : error));
                    return;
                }

                if (!string.Equals(state, expectedState, StringComparison.Ordinal))
                {
                    await WriteHtmlAsync(context, BuildResultPage(false, "Phiên đăng nhập không hợp lệ. Vui lòng thử lại."));
                    tcs.TrySetException(new InvalidOperationException("State OAuth không khớp."));
                    return;
                }

                if (string.IsNullOrEmpty(code))
                {
                    await WriteHtmlAsync(context, BuildResultPage(false, "Không nhận được mã xác thực từ nhà cung cấp."));
                    tcs.TrySetException(new InvalidOperationException("Thiếu authorization code."));
                    return;
                }

                await WriteHtmlAsync(context, BuildResultPage(true, "Bạn có thể quay lại ứng dụng Thư viện UIT."));
                tcs.TrySetResult(code);
            });

            using var registration = cancellationToken.Register(() =>
                tcs.TrySetCanceled(cancellationToken));

            try
            {
                await app.StartAsync(cancellationToken);

                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                return await tcs.Task.WaitAsync(cancellationToken);
            }
            catch (IOException ex) when (IsPortInUse(ex))
            {
                throw new InvalidOperationException(
                    $"Cổng {settings.RedirectPort} đang được sử dụng. Đổi RedirectPort trong appsettings.Development.json.",
                    ex);
            }
            catch (InvalidOperationException ex) when (IsHttpsCertificateIssue(ex))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình chứng chỉ HTTPS local. Chạy lệnh: dotnet dev-certs https --trust",
                    ex);
            }
            finally
            {
                await app.StopAsync(CancellationToken.None);
                await app.DisposeAsync();
            }
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "/oauth/callback";

            return path.StartsWith('/') ? path : "/" + path;
        }

        private static async Task WriteHtmlAsync(HttpContext context, string html)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(html, Encoding.UTF8);
        }

        private static string BuildResultPage(bool success, string message)
        {
            var title = success ? "Đăng nhập thành công" : "Đăng nhập thất bại";
            var color = success ? "#10B981" : "#EF4444";
            var encodedMessage = WebUtility.HtmlEncode(message);
            return "<!DOCTYPE html>" +
                   "<html lang=\"vi\"><head><meta charset=\"utf-8\"/>" +
                   $"<title>{title}</title>" +
                   "<style>" +
                   "body{font-family:Segoe UI,sans-serif;background:linear-gradient(135deg,#0D1B6E,#1E88E5);color:#fff;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;}" +
                   ".card{background:rgba(255,255,255,0.12);border:1px solid rgba(255,255,255,0.25);border-radius:16px;padding:32px 40px;max-width:420px;text-align:center;}" +
                   $"h1{{color:{color};margin-top:0;font-size:24px;}}" +
                   "p{line-height:1.6;opacity:0.9;}" +
                   "</style></head><body>" +
                   $"<div class=\"card\"><h1>{title}</h1><p>{encodedMessage}</p><p>Bạn có thể đóng tab này.</p></div>" +
                   "</body></html>";
        }

        private static bool IsPortInUse(IOException ex)
        {
            return ex.InnerException is SocketException { SocketErrorCode: SocketError.AddressAlreadyInUse };
        }

        private static bool IsHttpsCertificateIssue(Exception ex)
        {
            var message = ex.ToString();
            return message.Contains("certificate", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("HTTPS", StringComparison.OrdinalIgnoreCase);
        }
    }
}
