using MusicDownloadLib;
using System.Net.NetworkInformation;
using System.Threading.Channels;    

internal class Program
{
    private static async Task Main(string[] args)
    {
        //IChannel channel = new MusicDownloadLib.Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w", "neffexmusic", "UCoRR6OLuIZ2-5VxtnQIaN2w");
        IChannel channel = new MusicDownloadLib.Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");


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
    }
}
