using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MusicDownloadLib
{
    public static class PlaylistItemExtension
    {
        private static readonly string outputFilePath = @"C:\Users\Franek\OneDrive - BAKK Sp. z o.o\Pulpit\Joanna Chmilelewska\"; // Final merged file

        public static async Task<string> DownloadAsync(this PlaylistItem videoData) 
        {
            string result = "";
            var youtube = new YoutubeClient();
            try
            {
                // Get stream manifest
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync("https://www.youtube.com/watch?v="+videoData.Snippet.ResourceId.VideoId);

                // Select the best audio-only stream
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                if (audioStreamInfo == null)
                {
                    Console.WriteLine("Could not find suitable audio streams.");
                    return "Could not find suitable audio streams.";
                }

                var video = await youtube.Videos.GetAsync("https://www.youtube.com/watch?v=" + videoData.Snippet.ResourceId.VideoId);

                // Get the video title
                string videoTitle = Regex.Replace(video.Title, "[\\\\/:*?\"<>|]", "");
                string audioFilePath = Path.Combine(outputFilePath, videoTitle + ".mp4");
                string shorterTitle = videoTitle.Length > 25 ? videoTitle.Substring(0, 25) + "..." : videoTitle;
                // Create a progress handler
                var progressHandler = new Progress<double>(progress =>
                {
                    // Display the progress percentage
                    Console.Write($"\rDownloading {shorterTitle}: {progress:P0}");
                });

                // Download audio stream with progress
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath, progressHandler);

                Console.WriteLine($"\nDownload completed: {audioFilePath}");
                result = $"\nDownload completed: {audioFilePath}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }
            return result;
        }
    }
}
