using MusicDownloadLib.Channels;

namespace DOPOBIERANIAZYT
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IChannel channel = new Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");

            Console.WriteLine($"Channel: {channel.name} ({channel.id})");
            Console.WriteLine();
            Console.WriteLine("Uploads:");
            channel.videos.ForEach(v => Console.WriteLine(v.Snippet?.Title));

            Console.WriteLine();
            Console.WriteLine("Playlists:");
            int numerator = 0;
            channel.playlists.ForEach(p => Console.WriteLine(numerator++ + " " + p.GetTitle()));
        }
    }
}
