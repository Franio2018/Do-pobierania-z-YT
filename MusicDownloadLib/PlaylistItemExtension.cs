using Google.Apis.YouTube.v3.Data;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MusicDownloadLib
{
    public static class PlaylistItemExtension
    {
        private static readonly YoutubeClient Youtube = new();
        private static readonly Regex InvalidFileNameChars = new("[\\\\/:*?\"<>|]", RegexOptions.Compiled);

        public static string OutputDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "YouTube Downloads");

        public static async Task DownloadAsync(this PlaylistItem videoData, CancellationToken cancellationToken = default)
        {
            var videoId = videoData.Snippet?.ResourceId?.VideoId;
            if (string.IsNullOrEmpty(videoId))
                throw new InvalidOperationException("Playlist item has no video ID.");

            Directory.CreateDirectory(OutputDirectory);

            var videoUrl = "https://www.youtube.com/watch?v=" + videoId;
            var streamManifest = await Youtube.Videos.Streams.GetManifestAsync(videoUrl, cancellationToken);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate()
                ?? throw new InvalidOperationException("Could not find a suitable audio stream.");

            var video = await Youtube.Videos.GetAsync(videoUrl, cancellationToken);
            var videoTitle = SanitizeFileName(video.Title);
            var extension = audioStreamInfo.Container.Name;
            if (string.Equals(extension, "mp4", StringComparison.OrdinalIgnoreCase))
                extension = "m4a";

            var audioFilePath = Path.Combine(OutputDirectory, $"{videoTitle}.{extension}");
            var shorterTitle = videoTitle.Length > 25 ? videoTitle[..25] + "..." : videoTitle;

            var progressHandler = new Progress<double>(progress =>
            {
                Console.Write($"\rDownloading {shorterTitle}: {progress:P0}");
            });

            await Youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath, progressHandler, cancellationToken);
            Console.WriteLine($"\nDownload completed: {audioFilePath}");
        }

        private static string SanitizeFileName(string title)
        {
            var sanitized = InvalidFileNameChars.Replace(title, "").Trim();
            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "video";

            const int maxLength = 120;
            if (sanitized.Length > maxLength)
                sanitized = sanitized[..maxLength].Trim();

            return sanitized;
        }
    }
}
