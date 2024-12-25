using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MusicDownloadLib
{
    public class VideoMenagment
    {
        public async Task DownloadMusic(string videoUrl)
        {
            string outputFilePath = @"C:\Users\Franek\OneDrive - BAKK Sp. z o.o\Pulpit\MusicFranek\"; // Final merged file

            var youtube = new YoutubeClient();
            try
            {
                // Get stream manifest
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

                // Select the best audio-only stream
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (audioStreamInfo == null)
                {
                    Console.WriteLine("Could not find suitable audio streams.");
                    return;
                }

                var video = await youtube.Videos.GetAsync(videoUrl);

                // Get the video title
                string videoTitle = video.Title;
                string audioFilePath = Path.Combine(outputFilePath, videoTitle + ".mp4");

                // Create a progress handler
                var progressHandler = new Progress<double>(progress =>
                {
                    // Display the progress percentage
                    Console.Write($"\rDownloading {videoTitle}: {progress:P0}");
                });

                // Download audio stream with progress
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath, progressHandler);

                Console.WriteLine($"\nDownload completed: {audioFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public List<(string Tilte, string Url)> GetPlayListToDownload(string channelId, VideoGathering vidGad)
        {
            var list = vidGad.GetAllPlaylistsTitles(channelId);
            int i = 0;
            foreach (var item in list)
            {
                Console.WriteLine($"{i++}: " +  item.Title);
            }
            Console.WriteLine("Podaj Playlistę (NR) którą chcesz pobrać");
            int PlaylistNr = Convert.ToInt16(Console.ReadLine()); //Nr Playlisty wybranej przez użytkownika
            return vidGad.GetMusicFromPlaylist(list[PlaylistNr].Url);
        }
    }
}
