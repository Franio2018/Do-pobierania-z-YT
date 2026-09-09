using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace MusicDownloadLib
{
    internal static class YouTubeApi
    {
        private const string EnvVarName = "YOUTUBE_API_KEY";
        private const string KeyFileName = "youtube-api-key.txt";

        private static readonly Lazy<YouTubeService> Service = new(() =>
            new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = GetApiKey(),
                ApplicationName = "MusicDownloadLib"
            }));

        public static YouTubeService Client => Service.Value;

        internal static string GetApiKey()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvVarName);
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv.Trim();

            foreach (var directory in EnumerateSearchDirectories())
            {
                var path = Path.Combine(directory, KeyFileName);
                if (!File.Exists(path))
                    continue;

                var key = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(key))
                    return key;
            }

            throw new InvalidOperationException(
                $"YouTube API key is missing. Set the {EnvVarName} environment variable " +
                $"or create a {KeyFileName} file next to the app or in the solution folder.");
        }

        private static IEnumerable<string> EnumerateSearchDirectories()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var start in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
            {
                var current = new DirectoryInfo(start);
                while (current != null && seen.Add(current.FullName))
                {
                    yield return current.FullName;
                    current = current.Parent;
                }
            }
        }
    }
}
