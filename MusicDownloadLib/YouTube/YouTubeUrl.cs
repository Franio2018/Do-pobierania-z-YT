using System.Text.RegularExpressions;

namespace MusicDownloadLib.YouTube
{
    public static class YouTubeUrl
    {
        private static readonly Regex VideoIdPattern = new(@"^[\w-]{11}$", RegexOptions.Compiled);

        public static bool TryGetVideoId(string? input, out string videoId)
        {
            videoId = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var value = input.Trim();
            if (VideoIdPattern.IsMatch(value))
            {
                videoId = value;
                return true;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && !Uri.TryCreate("https://" + value, UriKind.Absolute, out uri))
            {
                return false;
            }

            if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                var id = uri.AbsolutePath.Trim('/');
                if (id.Length > 11)
                    id = id[..11];
                if (VideoIdPattern.IsMatch(id))
                {
                    videoId = id;
                    return true;
                }
            }

            var queryId = GetQueryValue(uri.Query, "v");
            if (queryId is not null && VideoIdPattern.IsMatch(queryId))
            {
                videoId = queryId;
                return true;
            }

            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (!segments[i].Equals("shorts", StringComparison.OrdinalIgnoreCase)
                    && !segments[i].Equals("embed", StringComparison.OrdinalIgnoreCase)
                    && !segments[i].Equals("live", StringComparison.OrdinalIgnoreCase)
                    && !segments[i].Equals("v", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var candidate = segments[i + 1];
                if (candidate.Length > 11)
                    candidate = candidate[..11];
                if (VideoIdPattern.IsMatch(candidate))
                {
                    videoId = candidate;
                    return true;
                }
            }

            return false;
        }

        private static string? GetQueryValue(string query, string key)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            var trimmed = query.TrimStart('?');
            foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2);
                if (pair.Length == 2 && pair[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(pair[1]);
            }

            return null;
        }
    }
}
