using Google.Apis.YouTube.v3.Data;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MusicDownloadLib.Downloads
{
    public static class PlaylistItemExtension
    {
        private static readonly YoutubeClient Youtube = new();
        private static readonly Regex InvalidFileNameChars = new("[\\\\/:*?\"<>|]", RegexOptions.Compiled);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new(StringComparer.OrdinalIgnoreCase);

        public static string OutputDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "YouTube Downloads");

        public static string? GetVideoId(this PlaylistItem videoData) =>
            videoData.Snippet?.ResourceId?.VideoId;

        public static IReadOnlyList<PlaylistItem> DistinctByVideoId(this IEnumerable<PlaylistItem> videos) =>
            videos
                .Where(video => !string.IsNullOrEmpty(video.GetVideoId()))
                .DistinctBy(video => video.GetVideoId(), StringComparer.Ordinal)
                .ToList();

        public static async Task<string> GetTitleAsync(string videoId, CancellationToken cancellationToken = default)
        {
            var video = await Youtube.Videos.GetAsync(videoId, cancellationToken);
            return video.Title;
        }

        public static async Task<DownloadResult> DownloadAsync(this PlaylistItem videoData, CancellationToken cancellationToken = default)
        {
            var videoId = videoData.GetVideoId();
            if (string.IsNullOrEmpty(videoId))
                throw new InvalidOperationException("Playlist item has no video ID.");

            Directory.CreateDirectory(OutputDirectory);

            var videoUrl = "https://www.youtube.com/watch?v=" + videoId;
            var streamManifest = await Youtube.Videos.Streams.GetManifestAsync(videoUrl, cancellationToken);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate()
                ?? throw new InvalidOperationException("Could not find a suitable audio stream.");

            var video = await Youtube.Videos.GetAsync(videoUrl, cancellationToken);
            var extension = GetAudioExtension(audioStreamInfo.Container.Name);
            var audioFilePath = BuildOutputFilePath(video.Title, extension);

            var fileLock = FileLocks.GetOrAdd(audioFilePath, _ => new SemaphoreSlim(1, 1));
            await fileLock.WaitAsync(cancellationToken);
            try
            {
                if (File.Exists(audioFilePath))
                    return new DownloadResult(videoId, video.Title, audioFilePath, WroteNewFile: false);

                var shorterTitle = Path.GetFileNameWithoutExtension(audioFilePath);
                if (shorterTitle.Length > 25)
                    shorterTitle = shorterTitle[..25] + "...";

                var progressHandler = new Progress<double>(progress =>
                {
                    Console.Write($"\rDownloading {shorterTitle}: {progress:P0}");
                });

                var tempFilePath = audioFilePath + ".part";
                try
                {
                    await Youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempFilePath, progressHandler, cancellationToken);
                    File.Move(tempFilePath, audioFilePath, overwrite: true);
                }
                finally
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }

                Console.WriteLine($"\nDownload completed: {audioFilePath}");
                return new DownloadResult(videoId, video.Title, audioFilePath, WroteNewFile: true);
            }
            finally
            {
                fileLock.Release();
            }
        }

        public static string BuildOutputFilePath(string title, string extension)
        {
            var fileName = $"{SanitizeFileName(title)}.{extension}";
            return Path.Combine(OutputDirectory, fileName);
        }

        public static string SanitizeFileName(string title)
        {
            var sanitized = InvalidFileNameChars.Replace(title, "").Trim();
            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "video";

            const int maxLength = 120;
            if (sanitized.Length > maxLength)
                sanitized = sanitized[..maxLength].Trim();

            return sanitized;
        }

        public static string GetAudioExtension(string containerName) =>
            string.Equals(containerName, "mp4", StringComparison.OrdinalIgnoreCase)
                ? "m4a"
                : containerName;
    }
}
