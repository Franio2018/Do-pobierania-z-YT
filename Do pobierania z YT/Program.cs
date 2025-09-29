using MusicDownloadLib;
using System.Net.NetworkInformation;
using System.Threading.Channels;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IChannel channel = new MusicDownloadLib.Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");
        //IChannel channel = new MusicDownloadLib.Channel("https://www.youtube.com/@Kowi-v9j");

        channel.videos.ForEach(v => Console.WriteLine(v.Snippet.Title));


        return;

        int numerator = 0;
        channel.playlists.ForEach(p => Console.WriteLine(numerator++ + " " + p.apiPlaylist.Snippet.Title));
        int index = Convert.ToInt32(Console.ReadLine());
        Playlist playlist = channel.GetPlaylist(index);

        return;

        var semaphore = new SemaphoreSlim(5); // max 5 jednocześnie
        var tasks = playlist.GetVideos().Select(async v =>
        {
            await semaphore.WaitAsync();
            try
            {
                await v.DownloadAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        // Czekanie na wszystkie
        await Task.WhenAll(tasks);

        return;

        MusicMenagment musicMenagment = new MusicMenagment();
        MusicGathering musicGathering = new MusicGathering();
        MusicListManagment musicListMngmt = new MusicListManagment();

        //Console.WriteLine("Podaj ID/nazwe kanału:");
        //string channelId = Console.ReadLine();
        List<(string Title, string Url)> ListaUtworow = new List<(string Title, string Url)>(); //videoMenagment.GetPlayListToDownload(channelId, videoGathering);

        //musicListMngmt.WriteListOfSongs(ListaUtworow);

        Console.WriteLine("chciałbyś dodać piosenke do listy?");
        musicGathering.AddVideoToList(ListaUtworow, Console.ReadLine());

        musicListMngmt.WriteListOfSongs(ListaUtworow);

        InitiateDownload(musicMenagment, ListaUtworow);

        Console.WriteLine("Press any key to close application");
        Console.ReadLine();
    }

    private static void InitiateDownload(MusicMenagment videoMenagment, List<(string Title, string Url)> ListaUtworow)
    {
        string response;
        do
        {
            Console.WriteLine("Podaj który element lub czy całą listę pobrać (Y/N/Nr)");
            List<Task> tasks = new List<Task>();
            response = Console.ReadLine();
            if (string.IsNullOrEmpty(response))
            {
                Console.Write("Podaj wartość!");
                continue;
            }
            if (response.ToUpper() == "Y")
            {
                foreach (var videoUrl in ListaUtworow)
                {
                    tasks.Add(videoMenagment.DownloadMusic(videoUrl.Url));
                }
            }
            else if (response.ToUpper() == "N")
            {
                return;
            }
            else
            {
                int index = -1;
                if (Int32.TryParse(response, out index))
                    tasks.Add(videoMenagment.DownloadMusic(ListaUtworow[index].Url));
            }
            Task.WaitAll(tasks.ToArray());
            break;
        } while (true);
    }
}
