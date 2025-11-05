using MusicDownloadLib;
using System.Net.NetworkInformation;
using System.Threading.Channels;

namespace DOPOBIERANIAZYT
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IChannel channel = new MusicDownloadLib.Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");
            //IChannel channel = new MusicDownloadLib.Channel("https://www.youtube.com/@Kowi-v9j");

            channel.videos.ForEach(v => Console.WriteLine(v.Snippet.Title));

            int numerator = 0;
            channel.playlists.ForEach(p => Console.WriteLine(numerator++ + " " + p.GetTitle()));
            int index = Convert.ToInt32(Console.ReadLine());
            Playlist playlist = channel.GetPlaylist(index);

            playlist.GetVideos().ForEach(v => v.DownloadAsync().Wait());
        }
    }

    public class Test
    {
        public IChannel channel { get; private set; }

        public Test(string url)
        {
            channel = new MusicDownloadLib.Channel(url);
        }
    }
}
