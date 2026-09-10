using System.Net;
using System.Text;
using System.Text.Json;
using MusicDownloadLib.Library;
using MusicDownloadLib.YouTube;

namespace MusicDownloadUI
{
    internal sealed class ListenInboxServer : IDisposable
    {
        public const int Port = 17823;

        private readonly HttpListener _listener = new();
        private CancellationTokenSource? _cts;

        public event Action<ListenExportItem>? ListenReceived;

        public bool IsListening => _listener.IsListening;

        public void Start()
        {
            if (_listener.IsListening)
                return;

            _listener.Prefixes.Clear();
            _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
            _listener.Start();
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public void Dispose()
        {
            _cts?.Cancel();
            if (_listener.IsListening)
                _listener.Stop();
            _listener.Close();
            _cts?.Dispose();
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().WaitAsync(cancellationToken);
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested || !_listener.IsListening)
                {
                    return;
                }
                catch
                {
                    continue;
                }

                _ = Task.Run(() => Handle(context));
            }
        }

        private void Handle(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    Write(response, 204, "");
                    return;
                }

                var path = request.Url?.AbsolutePath.TrimEnd('/') ?? "";
                if (request.HttpMethod == "GET" && (path == "/health" || path == ""))
                {
                    Write(response, 200, """{"ok":true}""", "application/json");
                    return;
                }

                if (request.HttpMethod == "POST" && path == "/listened")
                {
                    using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var body = reader.ReadToEnd();
                    if (!TryParseItem(body, out var item) || item is null)
                    {
                        Write(response, 400, """{"ok":false,"error":"invalid listen"}""", "application/json");
                        return;
                    }

                    ListenReceived?.Invoke(item);
                    Write(response, 200, """{"ok":true}""", "application/json");
                    return;
                }

                Write(response, 404, """{"ok":false}""", "application/json");
            }
            catch
            {
                try { context.Response.Abort(); } catch { /* ignore */ }
            }
        }

        private static bool TryParseItem(string body, out ListenExportItem? item)
        {
            item = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    if (root.GetArrayLength() == 0)
                        return false;
                    root = root[0];
                }

                var videoId = root.TryGetProperty("videoId", out var idEl) ? idEl.GetString() : null;
                if (!YouTubeUrl.TryGetVideoId(videoId, out var parsedId))
                    return false;

                item = new ListenExportItem
                {
                    VideoId = parsedId,
                    Title = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() ?? parsedId : parsedId,
                    Url = root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : "https://www.youtube.com/watch?v=" + parsedId,
                    ListenCount = 1,
                    ListenedAt = DateTimeOffset.UtcNow.ToString("O")
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void Write(HttpListenerResponse response, int status, string body, string contentType = "text/plain")
        {
            response.StatusCode = status;
            response.ContentType = contentType;
            var bytes = Encoding.UTF8.GetBytes(body);
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes);
            response.OutputStream.Close();
        }
    }
}
