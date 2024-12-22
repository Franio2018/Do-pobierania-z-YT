using MediaToolkit.Model;
using MediaToolkit;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Text.RegularExpressions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //Console.WriteLine(@"https://www.youtube.com/watch?v=HoneirfJ6Mc\\nhttps://www.youtube.com/watch?v=jO6Dgzu5bLk\\nhttps://www.youtube.com/watch?v=7dT89IEDx7w\\nhttps://www.youtube.com/watch?v=jWmKsq6VTKY\\nhttps://www.youtube.com/watch?v=1gYCL4TB0ao\\nhttps://www.youtube.com/watch?v=-o8KpfBiUcA\\nhttps://www.youtube.com/watch?v=gPclBFVqfNs\\nhttps://www.youtube.com/watch?v=03PFGEchCPw\\nhttps://www.youtube.com/watch?v=BO_Y-Cjeexs\\nhttps://www.youtube.com/watch?v=5O7vMoNVArE\\nhttps://www.youtube.com/watch?v=P6l5el2Y6vU\\nhttps://www.youtube.com/watch?v=vQY0hTrO3zU\\nhttps://www.youtube.com/watch?v=gRmC-OLKap8\\nhttps://www.youtube.com/watch?v=5MBI69YuEmI\\nhttps://www.youtube.com/watch?v=whMlYbKIfmU\\nhttps://www.youtube.com/watch?v=HUl6F72WQGg\\nhttps://www.youtube.com/watch?v=qexbUJjdS5o\\nhttps://www.youtube.com/watch?v=AIP4RV52YXw\\nhttps://www.youtube.com/watch?v=PQx3tpYkGxg\\nhttps://www.youtube.com/watch?v=DMKZvKtN8Bc\\nhttps://www.youtube.com/watch?v=RmuAjdOQ85g\\nhttps://www.youtube.com/watch?v=xMQ-IYGQWVg\\nhttps://www.youtube.com/watch?v=Jo5CCiY8_z4\\n");


        Console.WriteLine("Podana lista utowrów do pobrania:");
        StreamReader sr = new StreamReader("C:\\Users\\Franek\\source\\repos\\Do pobierania z YT\\Do pobierania z YT\\Linki.txt");
        string utwory = sr.ReadToEnd().Replace("\r", "");
        List<string> utworyLista = new List<string>();
        utworyLista = utwory.Split('\n').ToList();
        foreach (string s in utworyLista)
        {
            Console.WriteLine(s);
        }
        Console.WriteLine("Podane lista się zgadza (Y/N)");
        if (Console.ReadLine() == "Y")
        {
            await DownloadMusic(utworyLista);
        }
        else
        {
            return;
        }
    }
    private static async Task DownloadMusic(List<string> listaUtworow)
    {
        string outputFilePath = @"C:\Users\Franek\OneDrive - BAKK Sp. z o.o\Pulpit\Music\"; // Final merged file

        var youtube = new YoutubeClient();
        foreach (string videoUrl in listaUtworow)
        {
            try
            {
                // Get stream manifest
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

                // Select the best video-only stream
                var videoStreamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();

                // Select the best audio-only stream
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (videoStreamInfo == null || audioStreamInfo == null)
                {
                    Console.WriteLine("Could not find suitable streams for video or audio.");
                    return;
                }

                var video = await youtube.Videos.GetAsync(videoUrl);

                // Get the video title
                string videoTitle = video.Title;
                // Temporary file paths
                string audioFilePath = Path.Combine(outputFilePath, videoTitle);

                // Download audio stream
                Console.WriteLine("Downloading audio stream...");
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath+ ".mp4");

                Console.WriteLine($"Download and merge completed: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
