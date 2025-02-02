using MusicDownloadLib;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using VideoLibrary;

internal class Program
{
    private static async Task Main(string[] args)
    {
        VideoMenagment videoMenagment = new VideoMenagment();
        VideoGathering videoGathering = new VideoGathering();

        Console.WriteLine("Podaj ID/nazwe kanału:");
        string channelId = Console.ReadLine();
        List<(string Title, string Url)> ListaUtworow = videoMenagment.GetPlayListToDownload(channelId, videoGathering);

        WriteListOfSongs(ListaUtworow);

        //Console.WriteLine("chciałbyś dodać piosenke do listy?");
        //videoGathering.AddVideoToList(ListaUtworow, Console.ReadLine());

        //WriteListOfSongs(ListaUtworow);

        InitiateDownload(videoMenagment, ListaUtworow);
    }

    private static void InitiateDownload(VideoMenagment videoMenagment, List<(string Title, string Url)> ListaUtworow)
    {
        Console.WriteLine("Podaj który element lub czy całą listę pobrać (Y/N/Nr)");
        List<Task> tasks = new List<Task>();
        string response = Console.ReadLine();
        if (response == "Y")
        {
            foreach (var videoUrl in ListaUtworow)
            {
                tasks.Add(videoMenagment.DownloadMusic(videoUrl.Url));
            }
        }
        else if (response == "N")
        {
            return;
        }
        else
        {
            tasks.Add(videoMenagment.DownloadMusic(ListaUtworow[Convert.ToInt16(response)].Url));
        }
        Task.WaitAll(tasks.ToArray());
    }

    private static void WriteListOfSongs(List<(string Title, string Url)> ListaUtworow)
    {
        Console.WriteLine("Piosenki z playlisty:");
        int i = 0;
        foreach (var item in ListaUtworow)
        {
            Console.WriteLine($"{i++}: " + item.Title + " " + item.Url);
        }
    }
}
